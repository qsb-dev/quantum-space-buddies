using QSB.Events;
using QSB.Messaging;

namespace QSB.Animation
{
    public class PlayerSuitEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.SuitActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener(EventNames.SuitUp, () => SendEvent(CreateMessage(true)));
            GlobalMessenger.AddListener(EventNames.RemoveSuit, () => SendEvent(CreateMessage(false)));
        }

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            SenderId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            if (!IsInUniverse)
            {
                return;
            }
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.Suit, message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Suit, message.ToggleValue);
        }
    }
}
