using OWML.Common;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Components;
using QuantumUNET.Transport;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.TransformSync
{
	public abstract class QSBNetworkTransform : QNetworkTransform
	{
		public uint AttachedNetId => NetIdentity?.NetId.Value ?? uint.MaxValue;
		public uint PlayerId => NetIdentity.RootIdentity?.NetId.Value ?? NetIdentity.NetId.Value;
		public PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);

		public static List<QSBNetworkTransform> NetworkTransformList = new List<QSBNetworkTransform>();

		public QSBSector ReferenceSector { get; set; }
		public GameObject AttachedObject { get; set; }
		public SectorSync.SectorSync SectorSync { get; private set; }

		public abstract bool IsReady { get; }

		protected abstract GameObject InitLocalTransform();
		protected abstract GameObject InitRemoteTransform();

		private bool _isInitialized;
		private const float SmoothTime = 0.1f;
		protected virtual float DistanceLeeway { get; } = 5f;
		private float _previousDistance;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
		private IntermediaryTransform _intermediaryTransform;

		public virtual void Start()
		{
			var lowestBound = Resources.FindObjectsOfTypeAll<PlayerTransformSync>()
				.Where(x => x.NetId.Value <= NetId.Value).OrderBy(x => x.NetId.Value).Last();
			NetIdentity.SetRootIdentity(lowestBound.NetIdentity);

			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();

			NetworkTransformList.Add(this);
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
			NetworkTransformList.Remove(this);
			QSBSceneManager.OnSceneLoaded -= OnSceneLoaded;
			if (SectorSync != null)
			{
				Destroy(SectorSync);
			}
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse) =>
			_isInitialized = false;

		protected void Init()
		{
			AttachedObject = HasAuthority ? InitLocalTransform() : InitRemoteTransform();
			SetReferenceSector(SectorSync.GetClosestSector(AttachedObject.transform));
			_isInitialized = true;
		}

		public override void SerializeTransform(QNetworkWriter writer)
		{
			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}
			if (ReferenceSector != null)
			{
				writer.Write(ReferenceSector.ObjectId);
			}
			else
			{
				writer.Write(-1);
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
				reader.ReadInt32();
				reader.ReadVector3();
				DeserializeRotation(reader);
				return;
			}

			var sectorId = reader.ReadInt32();
			var sector = sectorId == -1 
				? null 
				: QSBWorldSync.GetWorldFromId<QSBSector>(sectorId);

			if (sector != ReferenceSector)
			{
				SetReferenceSector(sector);
			}

			var pos = reader.ReadVector3();
			var rot = DeserializeRotation(reader);

			if (HasAuthority)
			{
				return;
			}

			_intermediaryTransform.SetPosition(pos);
			_intermediaryTransform.SetRotation(rot);

			if (_intermediaryTransform.GetPosition() == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {PlayerId}.{GetType().Name} at (0,0,0)! - Given position was {pos} at sector {sector?.Name}", MessageType.Warning);
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
			}
			else
			{
				var targetPos = _intermediaryTransform.GetTargetPosition();
				var targetRot = _intermediaryTransform.GetTargetRotation();
				AttachedObject.transform.localPosition = SmartSmoothDamp(AttachedObject.transform.localPosition, targetPos);
				AttachedObject.transform.localRotation = QuaternionHelper.SmoothDamp(AttachedObject.transform.localRotation, targetRot, ref _rotationSmoothVelocity, SmoothTime);
			}
		}

		public override bool HasMoved()
		{
			var displacementMagnitude = (_intermediaryTransform.GetPosition() - _prevPosition).magnitude;
			return displacementMagnitude > 1E-03f
				|| Quaternion.Angle(_intermediaryTransform.GetRotation(), _prevRotation) > 1E-03f;
		}

		public void SetReferenceSector(QSBSector sector)
		{
			if (ReferenceSector == sector)
			{
				return;
			}
			ReferenceSector = sector;
			_intermediaryTransform.SetReferenceSector(sector);
			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Warning - AttachedObject was null for {PlayerId}.{GetType().Name} when trying to set reference sector to {sector.Name}. Waiting until not null...", MessageType.Warning);
				QSBCore.UnityEvents.RunWhen(
					() => AttachedObject != null,
					() => ReparentAttachedObject(sector.Transform));
				return;
			}
			if (!HasAuthority)
			{
				ReparentAttachedObject(sector.Transform);
			}
		}

		private void ReparentAttachedObject(Transform sectorTransform)
		{
			if (AttachedObject.transform.parent != null && AttachedObject.transform.parent.GetComponent<Sector>() == null)
			{
				DebugLog.ToConsole($" - ERROR - Trying to reparent attachedObject which wasnt attached to sector!", MessageType.Error);
				return;
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

			Popcron.Gizmos.Cube(_intermediaryTransform.GetTargetPosition(), _intermediaryTransform.GetTargetRotation(), Vector3.one / 2, Color.red);
			var color = HasMoved() ? Color.green : Color.yellow;
			Popcron.Gizmos.Cube(AttachedObject.transform.position, AttachedObject.transform.rotation, Vector3.one / 2, color);
			Popcron.Gizmos.Line(AttachedObject.transform.position, ReferenceSector.Transform.position, Color.cyan);
		}
	}
}
