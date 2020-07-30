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

        private uint GetAttachedNetId()
        { 
            /*
            Players are stored in PlayerRegistry using a specific ID. This ID has to remain the same
            for all components of a player, so I've chosen to used the netId of PlayerTransformSync.
            Since every networkbehaviour has it's own ascending netId, and we know that PlayerCameraSync
            is the 3rd network transform to be loaded (After PlayerTransformSync and ShipTransformSync),
            we can just minus 2 from PlayerCameraSync's netId to get PlayerTransformSyncs's netId.
            */
            return netId.Value - 2;
        }

        protected override Transform InitLocalTransform()
        {
            DebugLog.ToConsole($"Local PlayerCameraSync for {GetAttachedNetId()}");
            var body = Locator.GetPlayerCamera().gameObject.transform;

            PlayerToolsManager.Init(body.transform);

            PlayerRegistry.RegisterPlayerCamera(GetAttachedNetId(), body.gameObject);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            DebugLog.ToConsole($"Remote PlayerCameraSync for {GetAttachedNetId()}");
            var body = new GameObject("PlayerCamera");

            PlayerToolsManager.Init(body.transform);

            PlayerRegistry.RegisterPlayerCamera(GetAttachedNetId(), body);

            return body.transform;
        }

        protected override bool IsReady()
        {
            /*
            if (Locator.GetPlayerTransform() != null && PlayerRegistry.PlayerExists(GetAttachedNetId()))
            {
                OverriddenNetId = GetAttachedNetId();
                return true;
            }
            return false;
            */
            return Locator.GetPlayerTransform() != null && PlayerRegistry.PlayerExists(GetAttachedNetId());
        }
    }
}
