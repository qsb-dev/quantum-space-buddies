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
            GlobalMessenger<string, bool>.AddListener(EventNames.QSBGeyserState, (name, state) => SendEvent(CreateMessage(name, state)));
        }

        private GeyserMessage CreateMessage(string name, bool state) => new GeyserMessage
        {
            SenderId = PlayerRegistry.LocalPlayer.NetId,
            ObjectName = name,
            State = state
        };

        public override void OnReceiveRemote(GeyserMessage message)
        {
            if (!IsInUniverse)
            {
                return;
            }
            var geyser = WorldRegistry.GetObject<QSBGeyser>(message.ObjectName);
            geyser.SetState(message.State);
        }
    }
}
