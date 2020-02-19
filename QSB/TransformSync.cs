using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public abstract class TransformSync : NetworkBehaviour
    {
        private const float SmoothTime = 0.1f;

        private Transform _syncedTransform;
        private bool _isSectorSetUp;
        private Vector3 _positionSmoothVelocity;
        private Quaternion _rotationSmoothVelocity;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        protected abstract Transform GetLocalTransform();
        protected abstract Transform GetRemoteTransform();

        private void OnWakeUp()
        {
            DebugLog.Screen("Start TransformSync", netId.Value);
            Invoke(nameof(SetFirstSector), 1);

            transform.parent = Locator.GetRootTransform();
            _syncedTransform = hasAuthority ? GetLocalTransform() : GetRemoteTransform();
        }

        private void SetFirstSector()
        {
            _isSectorSetUp = true;
            SectorSync.SetSector(netId.Value, Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform);
        }

        public void EnterSector(Sector sector)
        {
            SectorSync.SetSector(netId.Value, sector.GetName());
        }

        private void Update()
        {
            if (!_syncedTransform || !_isSectorSetUp)
            {
                return;
            }

            var sectorTransform = SectorSync.GetSector(netId.Value);

            if (hasAuthority)
            {
                transform.position = sectorTransform.InverseTransformPoint(_syncedTransform.position);
                transform.rotation = sectorTransform.InverseTransformRotation(_syncedTransform.rotation);
            }
            else
            {
                _syncedTransform.parent = sectorTransform;

                _syncedTransform.localPosition = Vector3.SmoothDamp(_syncedTransform.localPosition, transform.position, ref _positionSmoothVelocity, SmoothTime);
                _syncedTransform.localRotation = Helpers.QuaternionSmoothDamp(_syncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);
            }
        }
    }
}
