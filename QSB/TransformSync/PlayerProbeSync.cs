using OWML.Common;
using QSB.Tools;
using QSB.Utility;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerProbeSync : TransformSync
    {
        public static PlayerProbeSync LocalInstance { get; private set; }

        private Transform _disabledSocket;

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        private Transform GetProbe()
        {
            return Locator.GetProbe().transform.Find("CameraPivot").Find("Geometry");
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetProbe();

            SetSocket(Player.Camera.transform);
            Player.ProbeBody = body.gameObject;

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var probe = GetProbe();

            if (probe == null)
            {
                DebugLog.ToConsole("Error - Probe is null!", MessageType.Error);
                return default;
            }

            var body = probe.InstantiateInactive();
            body.name = "RemoteProbeTransform";

            Destroy(body.GetComponentInChildren<ProbeAnimatorController>());

            PlayerToolsManager.CreateProbe(body, Player);

            QSB.Helper.Events.Unity.RunWhen(() => (Player.ProbeLauncher != null), () => SetSocket(Player.ProbeLauncher.ToolGameObject.transform));
            Player.ProbeBody = body.gameObject;

            return body;
        }

        private void SetSocket(Transform socket)
        {
            DebugLog.DebugWrite($"Setting DisabledSocket for {AttachedNetId} to {socket.name}");
            _disabledSocket = socket;
        }

        protected override void UpdateTransform()
        {
            base.UpdateTransform();
            if (Player == null)
            {
                DebugLog.ToConsole($"Player is null for {AttachedNetId}!", MessageType.Error);
                return;
            }
            if (ReferenceSector == null)
            {
                DebugLog.ToConsole($"ReferenceSector is null for {AttachedNetId}!", MessageType.Error);
                return;
            }
            if (_disabledSocket == null)
            {
                DebugLog.ToConsole($"DisabledSocket is null for {AttachedNetId}! (ProbeLauncher null? : {Player.ProbeLauncher == null})", MessageType.Error);
                return;
            }
            if (Player.GetState(State.ProbeActive) || ReferenceSector?.Sector == null)
            {
                return;
            }
            if (hasAuthority)
            {
                transform.position = ReferenceSector.Transform.InverseTransformPoint(_disabledSocket.position);
                return;
            }
            if (SyncedTransform.position == Vector3.zero ||
                SyncedTransform.position == Locator.GetAstroObject(AstroObject.Name.Sun).transform.position)
            {
                return;
            }
            SyncedTransform.localPosition = ReferenceSector.Transform.InverseTransformPoint(_disabledSocket.position);
        }

        public override bool IsReady => Locator.GetProbe() != null
            && Player != null
            && QSBPlayerManager.PlayerExists(Player.PlayerId)
            && Player.IsReady
            && netId.Value != uint.MaxValue
            && netId.Value != 0U;
    }
}
