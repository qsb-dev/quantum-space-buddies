using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.TransformSync
{
	public abstract class WorldObjectTransformSync : QNetworkBehaviour, ITransformSync
	{
		public abstract bool IsReady { get; }

		protected abstract IWorldObject GetWorldObject();

		public IWorldObject SyncedWorldObject { get; private set; }
		public QSBSector ReferenceSector { get; set; }
		public SectorSync.SectorSync SectorSync { get; private set; }
		public abstract SyncType SyncType { get; }

		private const float SmoothTime = 0.1f;
		private const float DistanceLeeway = 5f;
		private bool _isInitialized;
		private Vector3 _positionSmoothVelocity;
		private Quaternion _rotationSmoothVelocity;
		private bool _isVisible;
		private float _previousDistance;

		protected void Start()
		{
			DebugLog.DebugWrite($"Start of WorldObjectTransformSync");
			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();
			SectorSync.SetOwner(this);

			DontDestroyOnLoad(gameObject);
			QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
		}

		protected void OnDestroy()
		{
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
			SyncedWorldObject = GetWorldObject();
			SyncedWorldObject.TransformSync = this;
			DebugLog.DebugWrite($"Got SyncedWorldObject as {SyncedWorldObject.Name}");
			SetReferenceSector(SectorSync.GetClosestSector(SyncedWorldObject.ReturnObject().transform));
			_isInitialized = true;
			_isVisible = true;
		}

		public void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug || !IsReady)
			{
				return;
			}

			Popcron.Gizmos.Cube(SyncedWorldObject.ReturnObject().transform.position, SyncedWorldObject.ReturnObject().transform.rotation, Vector3.one / 2, Color.green); // Local object
			var targetPosition = ReferenceSector.Transform.TransformPoint(transform.position);
			var targetRotation = ReferenceSector.Transform.TransformRotation(transform.rotation);
			Popcron.Gizmos.Cube(targetPosition, targetRotation, Vector3.one / 2, Color.red); // Local object
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

			if (SyncedWorldObject == null)
			{
				DebugLog.ToConsole($"Warning - SyncedTransform for {GetType().Name} is null.", MessageType.Warning);
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
					DebugLog.ToConsole($"Error - ReferenceSector has null value for {SyncedWorldObject.Name}", MessageType.Error);
					return;
				}
				transform.position = ReferenceSector.Transform.InverseTransformPoint(SyncedWorldObject.ReturnObject().transform.position);
				transform.rotation = ReferenceSector.Transform.InverseTransformRotation(SyncedWorldObject.ReturnObject().transform.rotation);
				return;
			}

			// If this script is attached to any other body, eg the representations of other players
			if (SyncedWorldObject.ReturnObject().transform.position == Vector3.zero)
			{
				Hide();
			}
			else
			{
				Show();
			}

			var syncedTransform = SyncedWorldObject.ReturnObject().transform;

			syncedTransform.localPosition = SmartSmoothDamp(syncedTransform.localPosition, transform.position);
			syncedTransform.localRotation = QuaternionHelper.SmoothDamp(syncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, SmoothTime);
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
			DebugLog.DebugWrite($"Setting reference sector of {SyncedWorldObject.Name} to {sector.Name}");
			if (sector == ReferenceSector || sector == default(QSBSector))
			{
				return;
			}
			_positionSmoothVelocity = Vector3.zero;
			ReferenceSector = sector;
			if (!HasAuthority)
			{
				SyncedWorldObject.ReturnObject().transform.SetParent(sector.Transform, true);
				transform.position = sector.Transform.InverseTransformPoint(SyncedWorldObject.ReturnObject().transform.position);
				transform.rotation = sector.Transform.InverseTransformRotation(SyncedWorldObject.ReturnObject().transform.rotation);
			}
		}

		private void Show()
		{
			if (!_isVisible)
			{
				SyncedWorldObject.ReturnObject().gameObject.Show();
				_isVisible = true;
			}
		}

		private void Hide()
		{
			if (_isVisible)
			{
				SyncedWorldObject.ReturnObject().gameObject.Hide();
				_isVisible = false;
			}
		}
	}
}
