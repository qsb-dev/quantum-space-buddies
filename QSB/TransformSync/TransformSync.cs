using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public abstract class TransformSync : NetworkBehaviour
    {
        private const float SmoothTime = 0.1f;
        private static bool _isAwake;

        private Transform _syncedTransform;
        private bool _isSectorSetUp;
        private Vector3 _positionSmoothVelocity;
        private Quaternion _rotationSmoothVelocity;
        private Rigidbody _rigidBody;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);
            if (_isAwake)
            {
                OnWakeUp();
            }
            else
            {
                GlobalMessenger.AddListener("WakeUp", OnWakeUp);
            }
        }

        protected abstract Transform InitLocalTransform();
        protected abstract Transform InitRemoteTransform();

        private void OnWakeUp()
        {
            _isAwake = true;
            DebugLog.Screen("Start TransformSync", netId.Value);
            Invoke(nameof(SetFirstSector), 1);

            transform.parent = Locator.GetRootTransform();
            _syncedTransform = hasAuthority ? InitLocalTransform() : InitRemoteTransform();

            if (!hasAuthority)
            {
                _rigidBody = new GameObject("PlayerBody").AddComponent<Rigidbody>();
                _rigidBody.useGravity = false;
                _rigidBody.isKinematic = true;
                _rigidBody.gameObject.AddComponent<OWRigidbody>();
                var collider = _rigidBody.gameObject.AddComponent<CapsuleCollider>();
                collider.radius = 1;
                collider.height = 2;
                collider.center = Vector3.up * 1;

                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
                capsule.parent = _rigidBody.transform;
                capsule.localScale = Vector3.up * 1;
                capsule.localRotation = Quaternion.identity;
                capsule.localScale = new Vector3(1, 2, 1);
                Destroy(capsule.GetComponent<BoxCollider>());
            }
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
            if (!_syncedTransform || !_isSectorSetUp)
            {
                return;
            }

            var sectorTransform = SectorSync.Instance.GetSector(netId.Value);

            if (hasAuthority)
            {
                transform.position = sectorTransform.InverseTransformPoint(_syncedTransform.position);
                transform.rotation = sectorTransform.InverseTransformRotation(_syncedTransform.rotation);
            }
            else
            {
                _syncedTransform.parent = sectorTransform;

                _syncedTransform.localPosition = Vector3.SmoothDamp(_syncedTransform.localPosition, transform.position, ref _positionSmoothVelocity, SmoothTime);
                _syncedTransform.localRotation = QuaternionHelper.SmoothDamp(_syncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);

                _rigidBody.MovePosition(_syncedTransform.position);
                _rigidBody.MoveRotation(_syncedTransform.rotation);
            }
        }
    }
}
