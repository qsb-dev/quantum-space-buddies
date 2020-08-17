using QSB.Events;
using QSB.Messaging;

namespace QSB.Tools
{
    public class PlayerSignalscopeEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.SignalscopeActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<Signalscope>.AddListener(EventNames.EquipSignalscope, var => SendEvent(CreateMessage(true)));
            GlobalMessenger.AddListener(EventNames.UnequipSignalscope, () => SendEvent(CreateMessage(false)));
        }

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            SenderId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.Signalscope, message.ToggleValue);
            player.Signalscope?.ChangeEquipState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Signalscope, message.ToggleValue);
        }
    }
}
