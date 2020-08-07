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

        protected override uint PlayerId => netId.Value - 3;

        private Transform GetProbe()
        {
            return Locator.GetProbe().transform.Find("CameraPivot").Find("Geometry");
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetProbe();

            bodyTransform = body;

            Player.ProbeBody = body.gameObject;

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetProbe());

            PlayerToolsManager.CreateProbe(body, PlayerId);

            bodyTransform = body;

            Player.ProbeBody = body.gameObject;

            return body;
        }

        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            if (Player.GetState(State.ProbeActive))
            {
                return;
            }
            if (hasAuthority)
            {
                transform.position = ReferenceTransform.InverseTransformPoint(Player.ProbeLauncher.transform.position);
            }
            else
            {
                SyncedTransform.localPosition = ReferenceTransform.InverseTransformPoint(Player.ProbeLauncher.transform.position);
            }
        }

        protected override bool IsReady => Locator.GetProbe() != null && Player != null;
    }
}
