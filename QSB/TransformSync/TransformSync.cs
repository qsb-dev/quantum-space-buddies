using OWML.Common;
using QSB.Player;
using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.TransformSync
{
	public abstract class TransformSync : PlayerSyncObject
	{
		public abstract bool IsReady { get; }

		protected abstract Transform InitLocalTransform();
		protected abstract Transform InitRemoteTransform();

		public Transform SyncedTransform { get; private set; }
		public QSBSector ReferenceSector { get; set; }

		private const float SmoothTime = 0.1f;
		private const float DistanceLeeway = 5f;
		private bool _isInitialized;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
		private bool _isVisible;
		private float _previousDistance;

		protected override void Start()
		{
			base.Start();
			var lowestBound = QSBPlayerManager.GetSyncObjects<PlayerTransformSync>()
				.Where(x => x.NetId.Value <= NetId.Value).OrderBy(x => x.NetId.Value).Last();
			NetIdentity.SetRootIdentity(lowestBound.NetIdentity);

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
		}

		private void OnSceneLoaded(OWScene scene, bool isInUniverse) =>
			_isInitialized = false;

		protected void Init()
		{
			SyncedTransform = HasAuthority ? InitLocalTransform() : InitRemoteTransform();
			SetReferenceSector(QSBSectorManager.Instance.GetClosestSector(SyncedTransform));
			_isInitialized = true;
			_isVisible = true;
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
			if (HasAuthority) // If this script is attached to the client's own body on the client's side.
			{
				if (ReferenceSector == null || ReferenceSector.AttachedObject == null)
				{
					DebugLog.ToConsole($"Error - ReferenceSector has null value for {Player.PlayerId}.{GetType().Name}", MessageType.Error);
					return;
				}
				transform.position = ReferenceSector.Transform.InverseTransformPoint(SyncedTransform.position);
				transform.rotation = ReferenceSector.Transform.InverseTransformRotation(SyncedTransform.rotation);
				return;
			}

			// If this script is attached to any other body, eg the representations of other players
			if (SyncedTransform.position == Vector3.zero)
			{
				Hide();
			}
			else
			{
				Show();
			}

			SyncedTransform.localPosition = SmartSmoothDamp(SyncedTransform.localPosition, transform.position);
			SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, SmoothTime);
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