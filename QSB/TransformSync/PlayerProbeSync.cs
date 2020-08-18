using QSB.Tools;
using QSB.Utility;
using System;
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

        public override uint PlayerId
        {
            get
            {
                uint id = uint.MaxValue;
                try
                {
                    id = netId.Value - 3;
                }
                catch
                {
                    DebugLog.ToConsole($"Error while geting netId of {GetType().Name}! " +
                        $"{Environment.NewLine}     - Did you destroy the TransformSync without destroying the {GetType().Name}?" +
                        $"{Environment.NewLine}     - Did a destroyed TransformSync/{GetType().Name} still have an active action/event listener?" +
                        $"{Environment.NewLine}     If you are a user seeing this, please report this error.", OWML.Common.MessageType.Error);
                }
                return id;
            }
        }

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
            var probe = GetProbe();

            probe.gameObject.SetActive(false);
            var body = Instantiate(probe);
            probe.gameObject.SetActive(true);

            Destroy(body.GetComponentInChildren<ProbeAnimatorController>());

            PlayerToolsManager.CreateProbe(body, Player);

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
                transform.position = ReferenceSector.Transform.InverseTransformPoint(Player.ProbeLauncher.ToolGameObject.transform.position);
                return;
            }
            if (SyncedTransform.position == Vector3.zero ||
                SyncedTransform.position == Locator.GetAstroObject(AstroObject.Name.Sun).transform.position)
            {
                return;
            }
            SyncedTransform.localPosition = ReferenceSector.Transform.InverseTransformPoint(Player.ProbeLauncher.ToolGameObject.transform.position);
        }

        public override bool IsReady => Locator.GetProbe() != null && Player != null && Player.IsReady;
    }
}
