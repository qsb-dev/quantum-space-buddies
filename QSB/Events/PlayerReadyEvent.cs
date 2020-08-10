using QSB.Messaging;

namespace QSB.Events
{
    public class PlayerReadyEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.PlayerReady;

        public override void SetupListener()
        {
            GlobalMessenger<bool>.AddListener("QSBPlayerReady", ready => SendEvent(CreateMessage(ready)));
        }

        private ToggleMessage CreateMessage(bool ready) => new ToggleMessage
        {
            SenderId = LocalPlayerId,
            ToggleValue = ready
        };

        public override void OnServerReceive(ToggleMessage message)
        {
            PlayerRegistry.GetPlayer(message.SenderId).IsReady = message.ToggleValue;
            PlayerState.LocalInstance.Send();
        }
    }
}
