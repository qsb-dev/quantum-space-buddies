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
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    ToggleValue = true
                }));
            GlobalMessenger.AddListener("RemoveSuit", () => SendEvent(
                new ToggleMessage
                {
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    ToggleValue = false
                }));
        }

        public override void OnReceive(ToggleMessage message)
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
