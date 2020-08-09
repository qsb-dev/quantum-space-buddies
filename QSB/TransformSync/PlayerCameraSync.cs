using QSB.Tools;
using QSB.Utility;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerCameraSync : TransformSync
    {
        public static PlayerCameraSync LocalInstance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        protected override uint PlayerId => netId.Value - 2;

        protected override Transform InitLocalTransform()
        {
            DebugLog.ToConsole("Local of playercamerasync " + PlayerId);
            var body = Locator.GetPlayerCamera().gameObject.transform;

            PlayerToolsManager.Init(body);

            Player.Camera = body.gameObject;

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            DebugLog.ToConsole("Remote of playercamerasync " + PlayerId);
            var body = new GameObject("PlayerCamera");

            PlayerToolsManager.Init(body.transform);

            Player.Camera = body;

            return body.transform;
        }

        protected override bool IsReady => Locator.GetPlayerTransform() != null && Player != null;
    }
}
