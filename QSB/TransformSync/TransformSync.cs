using OWML.Common;
using QSB.Events;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public abstract class TransformSync : NetworkBehaviour
    {
        public PlayerInfo Player => PlayerRegistry.GetPlayer(PlayerId);

        private const float SmoothTime = 0.1f;

        public Transform SyncedTransform { get; private set; }
        public Transform ReferenceTransform { get; set; }

        private Vector3 _positionSmoothVelocity;
        private Quaternion _rotationSmoothVelocity;

        protected virtual void Awake()
        {
            PlayerRegistry.TransformSyncs.Add(this);
            DontDestroyOnLoad(gameObject);
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        private void OnCompleteSceneLoad(OWScene oldScene, OWScene newScene)
        {
            if (newScene == OWScene.SolarSystem || newScene == OWScene.EyeOfTheUniverse)
            {
                QSB.Helper.Events.Unity.FireOnNextUpdate(Init);
            }
        }

        protected abstract Transform InitLocalTransform();
        protected abstract Transform InitRemoteTransform();
        protected abstract bool IsReady { get; }
        protected abstract uint PlayerId { get; }

        private void Init()
        {
            ReferenceTransform = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
            SyncedTransform = hasAuthority ? InitLocalTransform() : InitRemoteTransform();
            if (!hasAuthority)
            {
                SyncedTransform.position = ReferenceTransform.position;
            }
        }

        private void Update()
        {
            if (!IsReady)
            {
                return;
            }
            UpdateTransform();
        }

        protected virtual void UpdateTransform()
        {
            if (hasAuthority) // If this script is attached to the client's own body on the client's side.
            {
                transform.position = ReferenceTransform.InverseTransformPoint(SyncedTransform.position);
                transform.rotation = ReferenceTransform.InverseTransformRotation(SyncedTransform.rotation);
                return;
            }

            // If this script is attached to any other body, eg the representations of other players
            if (SyncedTransform.position == Vector3.zero)
            {
                DebugLog.ToConsole($"TransformSync {netId.Value} at (0,0,0)", MessageType.Info);
                FullStateRequest.Instance.Request();
                return;
            }

            SyncedTransform.parent = ReferenceTransform;

            SyncedTransform.localPosition = Vector3.SmoothDamp(SyncedTransform.localPosition, transform.position, ref _positionSmoothVelocity, SmoothTime);
            SyncedTransform.localRotation = QuaternionHelper.SmoothDamp(SyncedTransform.localRotation, transform.rotation, ref _rotationSmoothVelocity, Time.deltaTime);
        }

    }
}
