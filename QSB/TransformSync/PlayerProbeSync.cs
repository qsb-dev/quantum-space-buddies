using QSB.Tools;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerProbeSync : TransformSync
    {
        public static PlayerProbeSync LocalInstance { get; private set; }

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
            Player.Probe = CreateProbe(body.gameObject, Player);
            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetProbe());
            Player.Probe = CreateProbe(body.gameObject, Player);
            Player.Probe.gameObject.SetActive(false);
            return body;
        }

        private QSBProbe CreateProbe(GameObject body, PlayerInfo player)
        {
            var probe = body.AddComponent<QSBProbe>();
            probe.Init(body, player, this);
            return probe;
        }

        protected override bool IsReady()
        {
            return Locator.GetProbe() != null && Player != null;
        }
    }
}
