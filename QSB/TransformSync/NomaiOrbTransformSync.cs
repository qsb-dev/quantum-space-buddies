using QSB.WorldSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public class NomaiOrbTransformSync : NetworkBehaviour
    {
        public NomaiInterfaceOrb AttachedOrb { get; private set; }
        private int Index => WorldRegistry.OrbList.FindIndex(x => x == this);

        public Transform OrbTransform { get; private set; }
        private bool _isInitialized;
        private bool _isReady;
        private Transform OrbParent;

        public override void OnStartClient()
        {
            WorldRegistry.OrbList.Add(this);

            QSB.Helper.Events.Unity.RunWhen(() => WorldRegistry.OldOrbList.Count != 0, OnReady);
        }

        private void OnReady()
        {
            AttachedOrb = WorldRegistry.OldOrbList[Index];
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
            OrbTransform = AttachedOrb.transform;
            OrbParent = AttachedOrb.GetAttachedOWRigidbody().GetOrigParent();
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

            if (OrbTransform == null || !_isInitialized)
            {
                return;
            }

            UpdateTransform();
        }

        protected virtual void UpdateTransform()
        {
            if (hasAuthority)
            {
                transform.position = OrbParent.InverseTransformPoint(OrbTransform.position);
                transform.rotation = OrbParent.InverseTransformRotation(OrbTransform.rotation);
                return;
            }
            OrbTransform.position = OrbParent.TransformPoint(transform.position);
            OrbTransform.rotation = OrbParent.InverseTransformRotation(OrbTransform.rotation);
        }
    }
}
