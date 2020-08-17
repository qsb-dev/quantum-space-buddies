using QSB.Events;
using QSB.Messaging;

namespace QSB.Tools
{
    public class PlayerSignalscopeEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.SignalscopeActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<Signalscope>.AddListener(EventNames.EquipSignalscope, HandleEquip);
            GlobalMessenger.AddListener(EventNames.UnequipSignalscope, HandleUnequip);
        }

        public override void CloseListener()
        {
            GlobalMessenger<Signalscope>.RemoveListener(EventNames.EquipSignalscope, HandleEquip);
            GlobalMessenger.RemoveListener(EventNames.UnequipSignalscope, HandleUnequip);
        }

        private void HandleEquip(Signalscope var) => SendEvent(CreateMessage(true));
        private void HandleUnequip() => SendEvent(CreateMessage(false));

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            FromId = LocalPlayerId,
            AboutId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.AboutId);
            player?.UpdateState(State.Signalscope, message.ToggleValue);
            if (!QSBSceneManager.IsInUniverse)
            {
                return;
            }
            player?.Signalscope?.ChangeEquipState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Signalscope, message.ToggleValue);
        }
    }
}
