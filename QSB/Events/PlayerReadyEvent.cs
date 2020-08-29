using OWML.Common;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using System.Linq;

namespace QSB.Events
{
    public class PlayerReadyEvent : QSBEvent<ToggleMessage>
    {
        public override EventType Type => EventType.PlayerReady;

        public override void SetupListener()
        {
            GlobalMessenger<bool>.AddListener(EventNames.QSBPlayerReady, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<bool>.RemoveListener(EventNames.QSBPlayerReady, Handler);
        }

        private void Handler(bool ready) => SendEvent(CreateMessage(ready));

        private ToggleMessage CreateMessage(bool ready) => new ToggleMessage
        {
            AboutId = LocalPlayerId,
            ToggleValue = ready
        };

        public override void OnServerReceive(ToggleMessage message)
        {
            DebugLog.DebugWrite($"[S] Get ready event from {message.FromId}", MessageType.Success);
            if (message.FromId == PlayerRegistry.LocalPlayerId)
            {
                return;
            }
            PlayerRegistry.GetPlayer(message.AboutId).IsReady = message.ToggleValue;
            PlayerState.LocalInstance.Send();
        }

        public override void OnReceiveRemote(ToggleMessage message)
        {
            DebugLog.DebugWrite($"Get ready event from {message.FromId}", MessageType.Success);
            foreach (var item in PlayerRegistry.GetSyncObjects<TransformSync.QSBTransformSync>()
                .Where(x => x != null && x.IsReady && x.ReferenceSector != null && x.PlayerId == LocalPlayerId))
            {
                DebugLog.DebugWrite($"* Sending sector for netid {item.netId.Value}...");
                GlobalMessenger<uint, QSBSector>.FireEvent(EventNames.QSBSectorChange, item.netId.Value, item.ReferenceSector);
            }
        }
    }
}
