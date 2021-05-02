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
	public abstract class UnparentedBaseTransformSync : QNetworkTransform
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

		private bool _isInitialized;
		private const float SmoothTime = 0.1f;
		protected virtual float DistanceLeeway { get; } = 5f;
		private float _previousDistance;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
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

		protected void OnSceneLoaded(OWScene scene, bool isInUniverse) =>
			_isInitialized = false;

		protected virtual void Init()
		{
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
				DebugLog.ToConsole($"Warning - {PlayerId}.{GetType().Name} at (0,0,0)! - Given position was {pos}", MessageType.Warning);
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
				DebugLog.ToConsole($"Warning - AttachedObject {Player.PlayerId}.{GetType().Name} is null.", MessageType.Warning);
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
			var targetPos = _intermediaryTransform.GetTargetPosition_Unparented();
			var targetRot = _intermediaryTransform.GetTargetRotation_Unparented();
			if (targetPos != Vector3.zero && _intermediaryTransform.GetTargetPosition_ParentedToReference() != Vector3.zero)
			{
				if (UseInterpolation)
				{
					AttachedObject.transform.position = SmartSmoothDamp(AttachedObject.transform.position, targetPos);
					AttachedObject.transform.rotation = QuaternionHelper.SmoothDamp(AttachedObject.transform.rotation, targetRot, ref _rotationSmoothVelocity, SmoothTime);
				}
				else
				{
					AttachedObject.transform.position = targetPos;
					AttachedObject.transform.rotation = targetRot;
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

			Popcron.Gizmos.Cube(_intermediaryTransform.GetTargetPosition_ParentedToReference(), _intermediaryTransform.GetTargetRotation_ParentedToReference(), Vector3.one / 2, Color.red);
			var color = HasMoved() ? Color.green : Color.yellow;
			Popcron.Gizmos.Cube(AttachedObject.transform.position, AttachedObject.transform.rotation, Vector3.one / 2, color);
			Popcron.Gizmos.Line(AttachedObject.transform.position, ReferenceTransform.position, Color.cyan);
		}
	}
}
