using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using System.Linq;

namespace QSB.Events
{
    public class PlayerStatesRequestEvent : QSBEvent<PlayerMessage>
    {
        public override MessageType Type => MessageType.FullStateRequest;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener(EventNames.QSBPlayerStatesRequest, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger.RemoveListener(EventNames.QSBPlayerStatesRequest, Handler);
        }

        private void Handler() => SendEvent(CreateMessage());

        private PlayerMessage CreateMessage() => new PlayerMessage
        {
            AboutId = LocalPlayerId
        };

        public override void OnServerReceive(PlayerMessage message)
        {
            DebugLog.ToConsole("Server get request for player states.", OWML.Common.MessageType.Warning);
            PlayerState.LocalInstance.Send();
            foreach (var item in PlayerRegistry.TransformSyncs.Where(x => x != null && x.IsReady && x.ReferenceSector != null))
            {
                DebugLog.ToConsole($"Sending sector event for {item.netId.Value}");
                GlobalMessenger<uint, QSBSector>.FireEvent(EventNames.QSBSectorChange, item.netId.Value, item.ReferenceSector);
            }
        }
    }
}
