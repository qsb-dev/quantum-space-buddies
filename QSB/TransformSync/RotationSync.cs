using QSB.Events;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace QSB.TransformSync
{
    public abstract class RotationSync : NetworkBehaviour
    {
        private const float SmoothTime = 0.1f;
        private bool _isInitialized;

        public Transform SyncedTransform { get; private set; }

        private Quaternion _rotationSmoothVelocity;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_isInitialized)
            {
                Reset();
            }
        }

        protected abstract Transform InitLocalTransform();
        protected abstract Transform InitRemoteTransform();
        protected abstract bool IsReady();

        protected void Init()
        {
            _isInitialized = true;

            SyncedTransform = hasAuthority ? InitLocalTransform() : InitRemoteTransform();
        }

        protected void Reset()
        {
            _isInitialized = false;
        }

        private void Update()
        {
            if (!_isInitialized && IsReady())
            {
                Init();
            }
            else if (_isInitialized && !IsReady())
            {
                Reset();
            }

            if (!SyncedTransform || !_isInitialized)
            {
                return;
            }

            if (hasAuthority) // If this script is attached to the client's own body on the client's side.
            {
                transform.localPosition = Vector3.zero;
                transform.rotation = SyncedTransform.rotation;
            }
            else // If this script is attached to any other body, eg the representations of other players
            {
                SyncedTransform.localPosition = Vector3.zero;
                SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);
            }
        }
    }
}
