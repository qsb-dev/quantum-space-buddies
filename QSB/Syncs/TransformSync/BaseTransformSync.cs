using OWML.Common;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;
using QuantumUNET.Components;
using QuantumUNET.Transport;
using System.Linq;
using UnityEngine;

namespace QSB.Syncs.TransformSync
{
	/*
	 * Rewrite number : 4
	 * God has cursed me for my hubris, and my work is never finished.
	 */

	public abstract class BaseTransformSync : QNetworkTransform
	{
		public uint AttachedNetId => NetIdentity?.NetId.Value ?? uint.MaxValue;
		public uint PlayerId => NetIdentity.RootIdentity?.NetId.Value ?? NetIdentity.NetId.Value;
		public PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);

		public Transform ReferenceTransform { get; set; }
		public GameObject AttachedObject { get; set; }

		public abstract bool IsReady { get; }
		public abstract bool UseInterpolation { get; }

		protected abstract GameObject InitLocalTransform();
		protected abstract GameObject InitRemoteTransform();

		protected bool _isInitialized;
		private const float SmoothTime = 0.1f;
		protected virtual float DistanceLeeway { get; } = 5f;
		private float _previousDistance;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
		private string _logName => $"{PlayerId}.{GetType().Name}";
		protected IntermediaryTransform _intermediaryTransform;

		public virtual void Start()
		{
			var lowestBound = Resources.FindObjectsOfTypeAll<PlayerTransformSync>()
				.Where(x => x.NetId.Value <= NetId.Value).OrderBy(x => x.NetId.Value).Last();
			NetIdentity.SetRootIdentity(lowestBound.NetIdentity);

			DontDestroyOnLoad(gameObject);
			_intermediaryTransform = new IntermediaryTransform(transform);
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		protected virtual void OnDestroy()
		{
			if (!HasAuthority && AttachedObject != null)
			{
				Destroy(AttachedObject);
			}
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse) 
			=> _isInitialized = false;

		protected virtual void Init()
		{
			if (!QSBSceneManager.IsInUniverse)
			{
				DebugLog.ToConsole($"Error - {_logName} is being init-ed when not in the universe!", MessageType.Error);
			}
			if (!HasAuthority && AttachedObject != null)
			{
				Destroy(AttachedObject);
			}
			AttachedObject = HasAuthority ? InitLocalTransform() : InitRemoteTransform();
			_isInitialized = true;
		}

		public override void SerializeTransform(QNetworkWriter writer)
		{
			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}

			var worldPos = _intermediaryTransform.GetPosition();
			var worldRot = _intermediaryTransform.GetRotation();
			writer.Write(worldPos);
			SerializeRotation(writer, worldRot);
			_prevPosition = worldPos;
			_prevRotation = worldRot;
		}

		public override void DeserializeTransform(QNetworkReader reader)
		{
			if (!QSBCore.HasWokenUp)
			{
				reader.ReadVector3();
				DeserializeRotation(reader);
				return;
			}

			var pos = reader.ReadVector3();
			var rot = DeserializeRotation(reader);

			if (HasAuthority)
			{
				return;
			}

			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}

			_intermediaryTransform.SetPosition(pos);
			_intermediaryTransform.SetRotation(rot);

			if (_intermediaryTransform.GetPosition() == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {_logName} at (0,0,0)! - Given position was {pos}", MessageType.Warning);
			}
		}

		public override void Update()
		{
			if (!_isInitialized && IsReady)
			{
				Init();
			}
			else if (_isInitialized && !IsReady)
			{
				_isInitialized = false;
				return;
			}

			if (!_isInitialized)
			{
				return;
			}

			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Warning - AttachedObject {_logName} is null.", MessageType.Warning);
				return;
			}

			if (!AttachedObject.activeInHierarchy)
			{
				return;
			}

			UpdateTransform();

			base.Update();
		}

		protected virtual void UpdateTransform()
		{
			if (HasAuthority)
			{
				_intermediaryTransform.EncodePosition(AttachedObject.transform.position);
				_intermediaryTransform.EncodeRotation(AttachedObject.transform.rotation);
				return;
			}
			var targetPos = _intermediaryTransform.GetTargetPosition_ParentedToReference();
			var targetRot = _intermediaryTransform.GetTargetRotation_ParentedToReference();
			if (targetPos != Vector3.zero && _intermediaryTransform.GetTargetPosition_Unparented() != Vector3.zero)
			{
				if (UseInterpolation)
				{
					AttachedObject.transform.localPosition = SmartSmoothDamp(AttachedObject.transform.localPosition, targetPos);
					AttachedObject.transform.localRotation = QuaternionHelper.SmoothDamp(AttachedObject.transform.localRotation, targetRot, ref _rotationSmoothVelocity, SmoothTime);
				}
				else
				{
					AttachedObject.transform.localPosition = targetPos;
					AttachedObject.transform.localRotation = targetRot;
				}
			}
		}

		public override bool HasMoved()
		{
			var displacementMagnitude = (_intermediaryTransform.GetPosition() - _prevPosition).magnitude;
			return displacementMagnitude > 1E-03f
				|| Quaternion.Angle(_intermediaryTransform.GetRotation(), _prevRotation) > 1E-03f;
		}

		public void SetReferenceTransform(Transform transform)
		{
			if (ReferenceTransform == transform)
			{
				return;
			}
			ReferenceTransform = transform;
			_intermediaryTransform.SetReferenceTransform(transform);
			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Warning - AttachedObject was null for {_logName} when trying to set reference transform to {transform.name}. Waiting until not null...", MessageType.Warning);
				QSBCore.UnityEvents.RunWhen(
					() => AttachedObject != null,
					() => ReparentAttachedObject(transform));
				return;
			}
			if (!HasAuthority)
			{
				ReparentAttachedObject(transform);
			}
		}

		private void ReparentAttachedObject(Transform sectorTransform)
		{
			if (AttachedObject.transform.parent != null && AttachedObject.transform.parent.GetComponent<Sector>() == null)
			{
				DebugLog.ToConsole($"Warning - Trying to reparent AttachedObject {AttachedObject.name} which wasnt attached to sector!", MessageType.Warning);
			}
			AttachedObject.transform.SetParent(sectorTransform, true);
			AttachedObject.transform.localScale = GetType() == typeof(PlayerTransformSync)
				? Vector3.one / 10
				: Vector3.one;
		}

		private Vector3 SmartSmoothDamp(Vector3 currentPosition, Vector3 targetPosition)
		{
			var distance = Vector3.Distance(currentPosition, targetPosition);
			if (distance > _previousDistance + DistanceLeeway)
			{
				_previousDistance = distance;
				return targetPosition;
			}
			_previousDistance = distance;
			return Vector3.SmoothDamp(currentPosition, targetPosition, ref _positionSmoothVelocity, SmoothTime);
		}

		private void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug || !IsReady)
			{
				return;
			}

			Popcron.Gizmos.Cube(_intermediaryTransform.GetTargetPosition_Unparented(), _intermediaryTransform.GetTargetRotation_Unparented(), Vector3.one / 2, Color.red);
			var color = HasMoved() ? Color.green : Color.yellow;
			Popcron.Gizmos.Cube(AttachedObject.transform.position, AttachedObject.transform.rotation, Vector3.one / 2, color);
			Popcron.Gizmos.Line(AttachedObject.transform.position, ReferenceTransform.position, Color.cyan);
		}
	}
}
