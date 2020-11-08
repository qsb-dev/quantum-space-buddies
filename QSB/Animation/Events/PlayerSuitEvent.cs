using QSB.EventsCore;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Animation
{
    public class PlayerSuitEvent : QSBEvent<ToggleMessage>
    {
        public override EventType Type => EventType.SuitActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener(EventNames.SuitUp, HandleSuitUp);
            GlobalMessenger.AddListener(EventNames.RemoveSuit, HandleSuitDown);
        }

        public override void CloseListener()
        {
            GlobalMessenger.RemoveListener(EventNames.SuitUp, HandleSuitUp);
            GlobalMessenger.RemoveListener(EventNames.RemoveSuit, HandleSuitDown);
        }

        private void HandleSuitUp() => SendEvent(CreateMessage(true));
        private void HandleSuitDown() => SendEvent(CreateMessage(false));

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            AboutId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = QSBPlayerManager.GetPlayer(message.AboutId);
            player?.UpdateState(State.Suit, message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            QSBPlayerManager.LocalPlayer.UpdateState(State.Suit, message.ToggleValue);
        }
    }
}