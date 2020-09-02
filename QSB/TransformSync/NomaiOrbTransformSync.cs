using QSB.WorldSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public class NomaiOrbTransformSync : NetworkBehaviour
    {
        private NomaiInterfaceOrb _attachedOrb;
        private int Index => WorldRegistry.OrbList.FindIndex(x => x == this);

        public Transform SyncedTransform { get; private set; }
        private bool _isInitialized;
        private bool _isReady;
        private Transform ReferenceTransform;
        private Vector3 _positionSmoothVelocity;
        private const float SmoothTime = 0.1f;

        public override void OnStartClient()
        {
            WorldRegistry.OrbList.Add(this);

            QSB.Helper.Events.Unity.RunWhen(() => WorldRegistry.OldOrbList.Count != 0, OnReady);
        }

        private void OnReady()
        {
            _attachedOrb = WorldRegistry.OldOrbList[Index];
            _isReady = true;
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool isInUniverse)
        {
            _isInitialized = false;
        }

        protected void Init()
        {
            SyncedTransform = _attachedOrb.transform;
            ReferenceTransform = _attachedOrb.GetAttachedOWRigidbody().GetOrigParent();
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized && _isReady)
            {
                Init();
            }
            else if (_isInitialized && !_isReady)
            {
                _isInitialized = false;
            }

            if (SyncedTransform == null || !_isInitialized)
            {
                return;
            }

            UpdateTransform();
        }

        protected virtual void UpdateTransform()
        {
            if (isServer)
            {
                transform.position = ReferenceTransform.InverseTransformPoint(SyncedTransform.position);
                transform.rotation = ReferenceTransform.InverseTransformRotation(SyncedTransform.rotation);
                return;
            }
            SyncedTransform.position = Vector3.SmoothDamp(SyncedTransform.position, ReferenceTransform.TransformPoint(transform.position), ref _positionSmoothVelocity, SmoothTime);
            SyncedTransform.rotation = ReferenceTransform.rotation;
        }
    }
}
