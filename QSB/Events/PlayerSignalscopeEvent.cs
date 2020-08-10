using QSB.Messaging;
using QSB.Utility;

namespace QSB.Events
{
    class PlayerSignalscopeEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.SignalscopeActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", var => SendEvent(
                new ToggleMessage {
                    SenderId = LocalPlayerId,
                    ToggleValue = true
                }));
            GlobalMessenger.AddListener("UnequipSignalscope", () => SendEvent(
                new ToggleMessage {
                    SenderId = LocalPlayerId,
                    ToggleValue = false
                }));
        }

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
