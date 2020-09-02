using QSB.Events;
using QSB.Tools;
using QSB.Utility;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public class PlayerCameraSync : TransformSync
    {
        public static PlayerCameraSync LocalInstance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        protected override Transform InitLocalTransform()
        {
            DebugLog.DebugWrite($"{MethodBase.GetCurrentMethod().Name} of {PlayerId}.{GetType().Name}");
            var body = Locator.GetPlayerCamera().gameObject.transform;

            Player.Camera = body.gameObject;

            Player.IsReady = true;
            GlobalMessenger<bool>.FireEvent(EventNames.QSBPlayerReady, true);
            DebugLog.DebugWrite("PlayerCameraSync init done - Request state!");
            GlobalMessenger.FireEvent(EventNames.QSBPlayerStatesRequest);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            DebugLog.DebugWrite($"{MethodBase.GetCurrentMethod().Name} of {PlayerId}.{GetType().Name}");
            var body = new GameObject("RemotePlayerCamera");

            PlayerToolsManager.Init(body.transform);

            Player.Camera = body;

            return body.transform;
        }

        public override bool IsReady => Locator.GetPlayerTransform() != null && PlayerRegistry.PlayerExists(PlayerId) && PlayerId != NetworkInstanceId.Invalid;
    }
}
