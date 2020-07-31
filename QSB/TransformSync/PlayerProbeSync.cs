using QSB.Animation;
using QSB.Tools;
using QSB.Utility;
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

        private uint GetAttachedNetId()
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

        private Transform GetProbeModel()
        {
            return Locator.GetProbe().transform.Find("CameraPivot").Find("Geometry");
        }

        protected override Transform InitLocalTransform()
        {
            DebugLog.ToConsole($"Local PlayerProbeSync for id {GetAttachedNetId()}");
            var body = GetProbeModel();

            bodyTransform = body;

            PlayerRegistry.GetPlayer(GetAttachedNetId()).ProbeBody = body.gameObject;

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            DebugLog.ToConsole($"Remote PlayerProbeSync for id {GetAttachedNetId()}");
            var body = Instantiate(GetProbeModel());

            bodyTransform = body;

            /*
            var oldProbe = body.GetComponent<SurveyorProbe>();
            var oldLantern = body.GetComponentInChildren<ProbeLantern>();
            var newProbe = body.gameObject.AddComponent<QSBProbe>();
            newProbe.Init(oldProbe, oldLantern);
            oldProbe.enabled = false;
            oldLantern.enabled = false;
            */

            PlayerRegistry.GetPlayer(GetAttachedNetId()).ProbeBody = body.gameObject;

            return body;
        }

        protected override bool IsReady()
        {
            return Locator.GetProbe() != null && PlayerRegistry.PlayerExists(GetAttachedNetId());
        }
    }
}
