using QSB.Events;
using QSB.Tools;
using QSB.Utility;
using System.Reflection;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerCameraSync : TransformSync
    {
        public static PlayerCameraSync LocalInstance { get; private set; }

        protected override uint PlayerIdOffset => 2;
        
        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }
        
        protected override Transform InitLocalTransform()
        {
            DebugLog.ToConsole($"{MethodBase.GetCurrentMethod().Name} for {GetType().Name}");
            var body = Locator.GetPlayerCamera().gameObject.transform;

            PlayerToolsManager.Init(body);

            Player.Camera = body.gameObject;

            DebugLog.ToConsole($"Set player {Player.NetId} to ready state true");
            Player.IsReady = true;
            GlobalMessenger<bool>.FireEvent(EventNames.QSBPlayerReady, true);
            DebugLog.ToConsole("Sending request for player states...", OWML.Common.MessageType.Warning);
            GlobalMessenger.FireEvent(EventNames.QSBPlayerStatesRequest);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            DebugLog.ToConsole($"{MethodBase.GetCurrentMethod().Name} for {GetType().Name}");
            var body = new GameObject("PlayerCamera");

            PlayerToolsManager.Init(body.transform);

            Player.Camera = body;

            return body.transform;
        }

        public override bool IsReady => Locator.GetPlayerTransform() != null && PlayerRegistry.PlayerExists(PlayerId);
    }
}
