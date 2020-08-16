using OWML.Common;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public abstract class TransformSync : NetworkBehaviour
    {
        public PlayerInfo Player => PlayerRegistry.GetPlayer(PlayerId);

        private const float SmoothTime = 0.1f;
        private bool _isInitialized;

        public Transform SyncedTransform { get; private set; }
        public QSBSector ReferenceSector { get; set; }

        private Vector3 _positionSmoothVelocity;
        private Quaternion _rotationSmoothVelocity;

        private bool _isVisible;

        protected virtual void Awake()
        {
            PlayerRegistry.TransformSyncs.Add(this);
            DontDestroyOnLoad(gameObject);
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool isInUniverse)
        {
            _isInitialized = false;
        }

        protected abstract Transform InitLocalTransform();
        protected abstract Transform InitRemoteTransform();
        public abstract bool IsReady { get; }
        public abstract uint PlayerId { get; }

        protected void Init()
        {
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
            }

            if (SyncedTransform == null || !_isInitialized)
            {
                return;
            }

            // Get which sector should be used as a reference point

            if (ReferenceSector == null)
            {
                DebugLog.ToConsole($"Error - TransformSync with id {netId.Value} doesn't have a reference sector", MessageType.Error);
            }

            UpdateTransform();
        }

        protected virtual void UpdateTransform()
        {
            if (hasAuthority) // If this script is attached to the client's own body on the client's side.
            {
                transform.position = ReferenceSector.Transform.InverseTransformPoint(SyncedTransform.position);
                transform.rotation = ReferenceSector.Transform.InverseTransformRotation(SyncedTransform.rotation);
                return;
            }

            // If this script is attached to any other body, eg the representations of other players
            if (SyncedTransform.position == Vector3.zero)
            {
                // Fix bodies staying at 0,0,0 by chucking them into the sun
                Hide();
                return;
            }

            Show();
            SyncedTransform.localPosition = Vector3.SmoothDamp(SyncedTransform.localPosition, transform.position, ref _positionSmoothVelocity, SmoothTime);
            SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);
        }

        public void SetReferenceSector(QSBSector sector)
        {
            _positionSmoothVelocity = Vector3.zero;
            ReferenceSector = sector;
            SyncedTransform.SetParent(sector.Transform, true);
            transform.position = sector.Transform.InverseTransformPoint(SyncedTransform.position);
            transform.rotation = sector.Transform.InverseTransformRotation(SyncedTransform.rotation);
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
