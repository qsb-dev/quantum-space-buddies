using OWML.Common;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Components;
using QuantumUNET.Transport;
using System.Linq;
using UnityEngine;

namespace QSB.TransformSync
{
	public abstract class QSBNetworkTransform : QNetworkTransform
	{
		public uint AttachedNetId => NetIdentity?.NetId.Value ?? uint.MaxValue;
		public uint PlayerId => NetIdentity.RootIdentity?.NetId.Value ?? NetIdentity.NetId.Value;
		public PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);

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

		public virtual void Start()
		{
			var lowestBound = Resources.FindObjectsOfTypeAll<PlayerTransformSync>()
				.Where(x => x.NetId.Value <= NetId.Value).OrderBy(x => x.NetId.Value).Last();
			NetIdentity.SetRootIdentity(lowestBound.NetIdentity);

			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();

			DontDestroyOnLoad(gameObject);
		}

		protected void Init()
		{
			AttachedObject = HasAuthority ? InitLocalTransform() : InitRemoteTransform();
			SetReferenceSector(SectorSync.GetClosestSector(AttachedObject.transform));
			_isInitialized = true;
		}

		public override void SerializeTransform(QNetworkWriter writer)
		{
			if (ReferenceSector != null)
			{
				writer.Write(ReferenceSector.ObjectId);
			}
			else
			{
				writer.Write(-1);
			}

			writer.Write(transform.localPosition);
			SerializeRotation(writer, transform.localRotation);
			_prevPosition = transform.localPosition;
			_prevRotation = transform.localRotation;
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

			var localPosition = reader.ReadVector3();
			var localRotation = DeserializeRotation(reader);

			if (HasAuthority)
			{
				return;
			}

			transform.localPosition = localPosition;
			transform.localRotation = localRotation;

			if (transform.position == Vector3.zero)
			{
				DebugLog.ToConsole($"Warning - {PlayerId}.{GetType().Name} at (0,0,0)! - Given localPosition was {localPosition} at sector {sector?.Name}", MessageType.Warning);
			}
		}

		public override void Update()
		{
			if (!_isInitialized && IsReady)
			{
				DebugLog.ToConsole($"Warning - {PlayerId}.{GetType().Name} is not initialized and ready.", MessageType.Warning);
				Init();
			}
			else if (_isInitialized && !IsReady)
			{
				DebugLog.ToConsole($"Warning - {PlayerId}.{GetType().Name} is initialized and not ready.", MessageType.Warning);
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
				transform.position = AttachedObject.transform.position;
				transform.rotation = AttachedObject.transform.rotation;
			}
			else
			{
				AttachedObject.transform.localPosition = SmartSmoothDamp(AttachedObject.transform.localPosition, transform.localPosition);
				AttachedObject.transform.localRotation = QuaternionHelper.SmoothDamp(AttachedObject.transform.localRotation, transform.localRotation, ref _rotationSmoothVelocity, SmoothTime);
			}
		}

		public override bool HasMoved()
		{
			var displacementMagnitude = (transform.localPosition - _prevPosition).magnitude;
			return displacementMagnitude > 1E-05f
				|| Quaternion.Angle(transform.localRotation, _prevRotation) > 1E-05f;
		}

		public void SetReferenceSector(QSBSector sector)
		{
			if (ReferenceSector == sector)
			{
				return;
			}
			ReferenceSector = sector;
			transform.SetParent(sector.Transform, true);
			if (!HasAuthority)
			{
				AttachedObject.transform.SetParent(sector.Transform, true);
				AttachedObject.transform.localScale = Vector3.one; // stop black hole weirdness?
			}
		}

		private Vector3 SmartSmoothDamp(Vector3 currentPosition, Vector3 targetPosition)
		{
			var distance = Vector3.Distance(currentPosition, targetPosition);
			if (distance > _previousDistance + DistanceLeeway)
			{
				// moved too far! assume sector sync warp / actual warp
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

			Popcron.Gizmos.Cube(transform.position, transform.rotation, Vector3.one / 2, Color.red);
			Popcron.Gizmos.Cube(AttachedObject.transform.position, AttachedObject.transform.rotation, Vector3.one / 2, Color.green);
			Popcron.Gizmos.Line(AttachedObject.transform.position, ReferenceSector.Transform.position, Color.cyan);
		}
	}
}
