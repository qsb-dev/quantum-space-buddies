using OWML.Common;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.TransformSync
{
	public abstract class SyncObjectTransformSync : PlayerSyncObject
	{
		public abstract bool IsReady { get; }

		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		public Transform SyncedTransform { get; private set; }
		public QSBSector ReferenceSector { get; set; }
		public SectorSync.SectorSync SectorSync { get; private set; }

		private const float SmoothTime = 0.1f;
		private const int DebugSavePosAmount = 75;
		public abstract float DistanceLeeway { get; }
		private bool _isInitialized;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
		private bool _isVisible;
		private float _previousDistance;
		private Queue<KeyValuePair<Vector3, Quaternion>> _previousPositionRotationData = new Queue<KeyValuePair<Vector3, Quaternion>>();

		protected override void Start()
		{
			base.Start();
			var lowestBound = QSBPlayerManager.GetSyncObjects<PlayerTransformSync>()
				.Where(x => x.NetId.Value <= NetId.Value).OrderBy(x => x.NetId.Value).Last();
			NetIdentity.SetRootIdentity(lowestBound.NetIdentity);

			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();

			DontDestroyOnLoad(gameObject);
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!HasAuthority && SyncedTransform != null)
			{
				Destroy(SyncedTransform.gameObject);
			}
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
			SyncedTransform = HasAuthority ? InitLocalTransform() : InitRemoteTransform();
			SetReferenceSector(SectorSync.GetClosestSector(SyncedTransform));
			_isInitialized = true;
			_isVisible = true;
		}

		public void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug || !IsReady)
			{
				return;
			}

			_previousPositionRotationData.Enqueue(new KeyValuePair<Vector3, Quaternion>(transform.position, transform.rotation));

			if (_previousPositionRotationData.Count >= DebugSavePosAmount)
			{
				_previousPositionRotationData.Dequeue();
			}

			for (var i = 0; i < _previousPositionRotationData.Count; i++)
			{
				var item = _previousPositionRotationData.ElementAt(i); // TODO : warning - O(n) not O(1) - maybe use array for O(1)?

				var targetPos = ReferenceSector.Transform.TransformPoint(item.Key);
				var targetRot = ReferenceSector.Transform.TransformRotation(item.Value);

				Popcron.Gizmos.Cube(targetPos, targetRot, Vector3.one / 2, new Color(1f, 0f, 0f, 1f - ((float)i).Map(0f, DebugSavePosAmount - 1, 1f, 0f)));

				if (i > 0)
				{
					var prevItem = _previousPositionRotationData.ElementAt(i - 1);
					var prevItemTargetPos = ReferenceSector.Transform.TransformPoint(prevItem.Key);
					Popcron.Gizmos.Line(prevItemTargetPos, targetPos, Color.yellow);
				}
			}

			Popcron.Gizmos.Cube(SyncedTransform.position, SyncedTransform.rotation, Vector3.one / 2, Color.green);
		}

		public void Update()
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

			if (SyncedTransform == null)
			{
				DebugLog.ToConsole($"Warning - SyncedTransform {Player.PlayerId}.{GetType().Name} is null.", MessageType.Warning);
				return;
			}

			UpdateTransform();
		}

		protected virtual void UpdateTransform()
		{
			var referenceSectorAtStart = ReferenceSector;

			if (HasAuthority) // If this script is attached to the client's own body on the client's side.
			{
				var previousPosition = transform.position;
				if (ReferenceSector == null || ReferenceSector.AttachedObject == null)
				{
					DebugLog.ToConsole($"Error - ReferenceSector has null value for {Player.PlayerId}.{GetType().Name}", MessageType.Error);
					return;
				}
				transform.position = ReferenceSector.Transform.InverseTransformPoint(SyncedTransform.position);
				if (transform.position == Vector3.zero)
				{
					DebugLog.ToConsole($"Error - {PlayerId}.{SyncedTransform.name} locally at (0, 0, 0)!", MessageType.Error);
				}
				transform.rotation = ReferenceSector.Transform.InverseTransformRotation(SyncedTransform.rotation);

				var distance = Vector3.Distance(previousPosition, transform.position);
				if (distance > _previousDistance + DistanceLeeway)
				{
					DebugLog.ToConsole($"Warning - {PlayerId}.{SyncedTransform.name} moved too far! Distance:{distance}, previous:{_previousDistance}, leeway:{DistanceLeeway}", MessageType.Warning);
				}
				_previousDistance = distance;

				if (referenceSectorAtStart != ReferenceSector)
				{
					DebugLog.ToConsole($"Warning - Reference for {PlayerId}.{SyncedTransform.name} changed while running UpdateTransform()!", MessageType.Warning);
				}

				return;
			}

			// If this script is attached to any other body, eg the representations of other players
			if (SyncedTransform.position == Vector3.zero || SyncedTransform.localPosition == Vector3.zero)
			{
				DebugLog.ToConsole($"Error - {PlayerId}.{SyncedTransform.name} at (0, 0, 0)!", MessageType.Error);
				Hide();
				return;
			}
			else
			{
				Show();
			}

			SyncedTransform.localPosition = SmartSmoothDamp(SyncedTransform.localPosition, transform.position);
			SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, SmoothTime);

			if (referenceSectorAtStart != ReferenceSector)
			{
				DebugLog.ToConsole($"Warning - Reference for {PlayerId}.{SyncedTransform.name} changed while running UpdateTransform()!", MessageType.Warning);
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

		public void SetReferenceSector(QSBSector sector)
		{
			if (sector == ReferenceSector || sector == default(QSBSector))
			{
				return;
			}
			DebugLog.DebugWrite($"{PlayerId}.{SyncedTransform.name} from:{(ReferenceSector == null ? "NULL" : ReferenceSector.Name)} to:{sector.Name}");
			_positionSmoothVelocity = Vector3.zero;
			ReferenceSector = sector;
			if (!HasAuthority)
			{
				SyncedTransform.SetParent(sector.Transform, true);
				transform.position = sector.Transform.InverseTransformPoint(SyncedTransform.position);
				transform.rotation = sector.Transform.InverseTransformRotation(SyncedTransform.rotation);
			}
		}

		private void Show()
		{
			if (!_isVisible)
			{
				SyncedTransform.gameObject.Show();
				_isVisible = true;
			}
		}

		private void Hide()
		{
			if (_isVisible)
			{
				SyncedTransform.gameObject.Hide();
				_isVisible = false;
			}
		}
	}
}