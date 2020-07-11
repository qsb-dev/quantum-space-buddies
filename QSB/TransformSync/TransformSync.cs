using QSB.Events;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace QSB.TransformSync
{
    public abstract class TransformSync : NetworkBehaviour
    {
        private const float SMOOTH_TIME = 0.1f;
        private bool _isInitialized;

        public Transform SyncedTransform { get; private set; }

        private bool _isSectorSetUp;
        private Vector3 _positionSmoothVelocity;
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
            Invoke(nameof(SetFirstSector), 1);

            SyncedTransform = hasAuthority ? InitLocalTransform() : InitRemoteTransform();
            if (!hasAuthority)
            {
                SyncedTransform.position = Locator.GetAstroObject(AstroObject.Name.Sun).transform.position;
            }
        }

        protected void Reset()
        {
            _isInitialized = false;
            _isSectorSetUp = false;
        }

        private void SetFirstSector()
        {
            _isSectorSetUp = true;
            Finder.UpdateSector(PlayerTransformSync.LocalInstance.netId.Value, Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform);
        }

        public void EnterSector(Sector sector)
        {
            //SectorSync.Instance.SetSector(netId.Value, sector.GetName());
            SectorSync.Instance.SetSector(PlayerTransformSync.LocalInstance.netId.Value, sector.GetName());
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

            if (!SyncedTransform || !_isSectorSetUp || !_isInitialized)
            {
                return;
            }

            // Get which sector should be used as a reference point
            //var sectorTransform = SectorSync.Instance.GetSector(netId.Value);
            //var sectorTransform = Finder.GetSector(netId.Value);
            var sectorTransform = Finder.GetSector(PlayerTransformSync.LocalInstance.netId.Value);

            if (hasAuthority) // If this script is attached to the client's own body on the client's side.
            {
                transform.position = sectorTransform.InverseTransformPoint(SyncedTransform.position);
                transform.rotation = sectorTransform.InverseTransformRotation(SyncedTransform.rotation);
            }
            else // If this script is attached to any other body, eg the representations of other players
            {
                if (SyncedTransform.position == Vector3.zero) 
                {
                    // Fix bodies staying at 0,0,0 by chucking them into the sun
                    SyncedTransform.position = Locator.GetAstroObject(AstroObject.Name.Sun).transform.position;

                    FullStateRequest.LocalInstance.Request();
                }
                else
                {
                    SyncedTransform.parent = sectorTransform;

                    SyncedTransform.localPosition = Vector3.SmoothDamp(SyncedTransform.localPosition, transform.position, ref _positionSmoothVelocity, SMOOTH_TIME);
                    SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);
                }
            }
        }
    }
}
