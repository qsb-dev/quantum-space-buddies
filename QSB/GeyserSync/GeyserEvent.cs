using QSB.Events;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.GeyserSync
{
    public class GeyserEvent : QSBEvent<GeyserMessage>
    {
        public override MessageType Type => MessageType.Geyser;

        public override void SetupListener()
        {
            GlobalMessenger<int, bool>.AddListener(EventNames.QSBGeyserState, (id, state) => SendEvent(CreateMessage(id, state)));
        }

        private GeyserMessage CreateMessage(int id, bool state) => new GeyserMessage
        {
            SenderId = PlayerRegistry.LocalPlayer.NetId,
            ObjectId = id,
            State = state
        };

        public override void OnReceiveRemote(GeyserMessage message)
        {
            if (!IsInUniverse)
            {
                return;
            }
            var geyser = WorldRegistry.GetObject<QSBGeyser>(message.ObjectId);
            geyser.SetState(message.State);
        }
    }
}
