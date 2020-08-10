using QSB.Messaging;

namespace QSB.Events
{
    public class PlayerSuitEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.SuitActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("SuitUp", () => SendEvent(CreateMessage(true)));
            GlobalMessenger.AddListener("RemoveSuit", () => SendEvent(CreateMessage(false)));
        }

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            SenderId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.Suit, message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Suit, message.ToggleValue);
        }
    }
}
