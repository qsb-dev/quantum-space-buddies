using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace QSB.TransformSync
{
    public abstract class TransformSync : NetworkBehaviour
    {
        private const float SmoothTime = 0.1f;
        protected bool _isInitialized;

        public Transform SyncedTransform { get; private set; }

        private bool _isSectorSetUp;
        private Vector3 _positionSmoothVelocity;
        private Quaternion _rotationSmoothVelocity;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);
        }

        protected abstract Transform InitLocalTransform();
        protected abstract Transform InitRemoteTransform();

        protected void Init()
        {
            _isInitialized = true;
            DebugLog.Screen("Start TransformSync", netId.Value);
            Invoke(nameof(SetFirstSector), 1);

            transform.parent = Locator.GetRootTransform();
            SyncedTransform = hasAuthority ? InitLocalTransform() : InitRemoteTransform();
            if (!hasAuthority)
            {
                SyncedTransform.position = Locator.GetAstroObject(AstroObject.Name.Sun).transform.position;
            }
        }

        protected void Reset()
        {
            _isInitialized = false;
        }

        private void SetFirstSector()
        {
            _isSectorSetUp = true;
            SectorSync.Instance.SetSector(netId.Value, Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform);
        }

        public void EnterSector(Sector sector)
        {
            SectorSync.Instance.SetSector(netId.Value, sector.GetName());
        }

        private void Update()
        {
            if (!SyncedTransform || !_isSectorSetUp)
            {
                return;
            }

            var sectorTransform = SectorSync.Instance.GetSector(netId.Value);

            if (hasAuthority)
            {
                transform.position = sectorTransform.InverseTransformPoint(SyncedTransform.position);
                transform.rotation = sectorTransform.InverseTransformRotation(SyncedTransform.rotation);
            }
            else
            {
                if (SyncedTransform.position == Vector3.zero)
                {
                    SyncedTransform.position = Locator.GetAstroObject(AstroObject.Name.Sun).transform.position;
                }
                else
                {
                    SyncedTransform.parent = sectorTransform;

                    SyncedTransform.localPosition = Vector3.SmoothDamp(SyncedTransform.localPosition, transform.position, ref _positionSmoothVelocity, SmoothTime);
                    SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);
                }
            }
        }
    }
}
