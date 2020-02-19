using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public abstract class TransformSync : NetworkBehaviour
    {
        private Transform _syncedTransform;
        private bool _isSectorSetUp;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        protected abstract Transform GetLocalPlayerTransform();
        protected abstract Transform GetRemotePlayerTransform();

        private void OnWakeUp()
        {
            DebugLog.Screen("Start TransformSync", netId.Value);
            Invoke(nameof(SetFirstSector), 1);

            transform.parent = Locator.GetRootTransform();
            _syncedTransform = isLocalPlayer ? GetLocalPlayerTransform() : GetRemotePlayerTransform();
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

            if (isLocalPlayer)
            {
                transform.position = sectorTransform.InverseTransformPoint(_syncedTransform.position);
                transform.rotation = sectorTransform.InverseTransformRotation(_syncedTransform.rotation);
            }
            else
            {
                _syncedTransform.parent = sectorTransform;
                _syncedTransform.position = sectorTransform.TransformPoint(transform.position);
                _syncedTransform.rotation = sectorTransform.rotation * transform.rotation;
            }
        }

    }
}
