using OWML.Common;
using QSB.Events;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace QSB.TransformSync
{
    public abstract class TransformSync : NetworkBehaviour
    {
        private const float SmoothTime = 0.1f;
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
        protected abstract uint GetAttachedNetId();
        protected abstract bool Override();

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
            PlayerRegistry.GetPlayer(GetAttachedNetId()).ReferenceSector = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
        }

        public void EnterSector(Sector sector)
        {
            SectorSync.Instance.SetSector(GetAttachedNetId(), sector.GetName());
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

            var sectorTransform = PlayerRegistry.GetPlayer(GetAttachedNetId()).ReferenceSector;

            if (hasAuthority) // If this script is attached to the client's own body on the client's side.
            {
                transform.position = sectorTransform.InverseTransformPoint(SyncedTransform.position);
                transform.rotation = sectorTransform.InverseTransformRotation(SyncedTransform.rotation);

                if (Override())
                {
                    transform.position = sectorTransform.InverseTransformPoint(PlayerRegistry.GetPlayer(GetAttachedNetId()).ProbeLauncher.transform.position);
                }
                else
                {
                    if (this.GetType().Name == "PlayerProbeSync")
                    {
                        DebugLog.ToConsole($"HAS AUTH {GetAttachedNetId()} : {sectorTransform.name} / {SyncedTransform.localPosition} -> {transform.position}", MessageType.Warning);
                    }
                }
            }
            else // If this script is attached to any other body, eg the representations of other players
            {
                if (SyncedTransform.position == Vector3.zero)
                {
                    // Fix bodies staying at 0,0,0 by chucking them into the sun
                    SyncedTransform.position = Locator.GetAstroObject(AstroObject.Name.Sun).transform.position;

                    DebugLog.ToConsole("Warning - TransformSync at (0,0,0)!", MessageType.Warning);

                    FullStateRequest.LocalInstance.Request();
                }
                else
                {
                    SyncedTransform.parent = sectorTransform;

                    SyncedTransform.localPosition = Vector3.SmoothDamp(SyncedTransform.localPosition, transform.position, ref _positionSmoothVelocity, SmoothTime);
                    SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);

                    if (Override())
                    {
                        SyncedTransform.localPosition = sectorTransform.InverseTransformPoint(PlayerRegistry.GetPlayer(GetAttachedNetId()).ProbeLauncher.transform.position);
                    }
                }
            }
        }
    }
}
