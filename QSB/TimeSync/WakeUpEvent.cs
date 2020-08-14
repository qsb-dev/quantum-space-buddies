using QSB.Events;
using QSB.Messaging;

namespace QSB.TimeSync
{
    public class WakeUpEvent : QSBEvent<PlayerMessage>
    {
        public override MessageType Type => MessageType.WakeUp;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener(EventNames.WakeUp, Handler);
        }

        private void Handler()
        {
            PlayerRegistry.LocalPlayer.IsAwake = true;
            SendEvent(new PlayerMessage
            {
                SenderId = PlayerRegistry.LocalPlayerId
            });
        }

        public override void OnReceiveRemote(PlayerMessage message)
        {
            PlayerRegistry.GetPlayer(message.SenderId).IsAwake = true;
        }
    }
}
