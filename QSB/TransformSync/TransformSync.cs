using System;
using OWML.Common;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public abstract class TransformSync : NetworkBehaviour
    {
        public abstract bool IsReady { get; }
        protected abstract Transform InitLocalTransform();
        protected abstract Transform InitRemoteTransform();
        protected abstract uint PlayerIdOffset { get; }

        public uint PlayerId => GetPlayerId();
        public PlayerInfo Player => PlayerRegistry.GetPlayer(PlayerId);
        public Transform SyncedTransform { get; private set; }
        public QSBSector ReferenceSector { get; set; }

        private const float SmoothTime = 0.1f;
        private bool _isInitialized;
        private Vector3 _positionSmoothVelocity;
        private Quaternion _rotationSmoothVelocity;
        private bool _isVisible;

        protected virtual void Awake()
        {
            DebugLog.ToConsole("Awake of " + GetType().Name, MessageType.Error);
            PlayerRegistry.TransformSyncs.Add(this);
            DontDestroyOnLoad(gameObject);
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool isInUniverse)
        {
            _isInitialized = false;
        }

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
                if (ReferenceSector.Sector == null)
                {
                    DebugLog.ToConsole($"Sector is null for referencesector for {GetType().Name}!", MessageType.Error);
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

        private uint GetPlayerId()
        {
            try
            {
                return netId.Value - PlayerIdOffset;
            }
            catch
            {
                DebugLog.ToConsole($"Error while getting netId of {GetType().Name}! " +
                                   $"{Environment.NewLine}     - Did you destroy the TransformSync without destroying the {GetType().Name}?" +
                                   $"{Environment.NewLine}     - Did a destroyed TransformSync/{GetType().Name} still have an active action/event listener?" +
                                   $"{Environment.NewLine}     If you are a user seeing this, please report this error.", MessageType.Error);
                return uint.MaxValue;
            }
        }

    }
}
