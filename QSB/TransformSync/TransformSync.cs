using OWML.Common;
using QSB.Utility;
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
        private bool _isInitialized;
        private Vector3 _positionSmoothVelocity;
        private Quaternion _rotationSmoothVelocity;
        private bool _isVisible;

        protected virtual void Awake()
        {
            DebugLog.DebugWrite($"Awake of {AttachedNetId} ({GetType().Name})");
            PlayerRegistry.PlayerSyncObjects.Add(this);
            DontDestroyOnLoad(gameObject);
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool isInUniverse)
        {
            _isInitialized = false;
        }

        protected void Init()
        {
            DebugLog.DebugWrite($"Init of {AttachedNetId} ({Player.PlayerId}.{GetType().Name})");
            ReferenceSector = QSBSectorManager.Instance.GetStartPlanetSector();
            SyncedTransform = hasAuthority ? InitLocalTransform() : InitRemoteTransform();
            if (!hasAuthority)
            {
                SyncedTransform.position = ReferenceSector.Position;
            }
            _isInitialized = true;
            _isVisible = true;
        }

        private void Update()
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
                DebugLog.ToConsole($"SyncedTransform {AttachedNetId} ({Player.PlayerId}.{GetType().Name}) is null!");
                return;
            }

            if (ReferenceSector == null)
            {
                DebugLog.ToConsole($"Error - {AttachedNetId} ({Player.PlayerId}.{GetType().Name}) doesn't have a reference sector", MessageType.Error);
            }

            UpdateTransform();
        }

        protected virtual void UpdateTransform()
        {
            if (hasAuthority) // If this script is attached to the client's own body on the client's side.	
            {
                if (ReferenceSector.Sector == null)
                {
                    DebugLog.ToConsole($"Sector is null for referencesector for {AttachedNetId} ({Player.PlayerId}.{GetType().Name})!", MessageType.Error);
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

            SyncedTransform.localPosition = Vector3.SmoothDamp(SyncedTransform.localPosition, transform.position, ref _positionSmoothVelocity, SmoothTime);
            SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);
        }

        public void SetReferenceSector(QSBSector sector)
        {
            _positionSmoothVelocity = Vector3.zero;
            ReferenceSector = sector;
            if (!hasAuthority)
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