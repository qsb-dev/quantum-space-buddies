using QSB.Messaging;

namespace QSB.Events
{
    class PlayerSuitEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.SuitActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("SuitUp", () => SendEvent(
                new ToggleMessage
                {
                    SenderId = LocalPlayerId,
                    ToggleValue = true
                }));
            GlobalMessenger.AddListener("RemoveSuit", () => SendEvent(
                new ToggleMessage
                {
                    SenderId = LocalPlayerId,
                    ToggleValue = false
                }));
        }

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
