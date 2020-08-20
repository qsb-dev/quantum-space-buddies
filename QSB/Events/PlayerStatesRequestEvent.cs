using QSB.Messaging;
using QSB.TransformSync;
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
            PlayerState.LocalInstance.Send();
            foreach (var item in PlayerRegistry.TransformSyncs.Where(x => x != null && x.IsReady && x.ReferenceSector != null))
            {
                GlobalMessenger<uint, QSBSector>.FireEvent(EventNames.QSBSectorChange, item.netId.Value, item.ReferenceSector);
            }
        }
    }
}
