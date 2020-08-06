using QSB.Tools;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerProbeSync : TransformSync
    {
        public static PlayerProbeSync LocalInstance { get; private set; }

        public Transform bodyTransform;

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        protected override uint GetAttachedNetId()
        {
            /*
            Players are stored in PlayerRegistry using a specific ID. This ID has to remain the same
            for all components of a player, so I've chosen to used the netId of PlayerTransformSync.
            Since every networkbehaviour has it's own ascending netId, and we know that PlayerCameraSync
            is the 4th network transform to be loaded (After PlayerTransformSync, ShipTransformSync and PlayerCameraSync),
            we can just minus 3 from PlayerProbeSync's netId to get PlayerTransformSyncs's netId.
            */
            return netId.Value - 3;
        }

        private Transform GetProbe()
        {
            return Locator.GetProbe().transform.Find("CameraPivot").Find("Geometry");
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetProbe();

            bodyTransform = body;

            PlayerRegistry.GetPlayer(GetAttachedNetId()).ProbeBody = body.gameObject;

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetProbe());

            PlayerToolsManager.CreateProbe(body, GetAttachedNetId());

            bodyTransform = body;

            PlayerRegistry.GetPlayer(GetAttachedNetId()).ProbeBody = body.gameObject;

            return body;
        }

        protected override Vector3? Override()
        {
            var player = PlayerRegistry.GetPlayer(GetAttachedNetId());
            if (player.GetState(State.ProbeActive))
            {
                return null;
            }
            return PlayerRegistry.GetPlayer(GetAttachedNetId()).ProbeLauncher.transform.position;
        }

        protected override bool IsReady()
        {
            return Locator.GetProbe() != null && PlayerRegistry.PlayerExists(GetAttachedNetId());
        }
    }
}
