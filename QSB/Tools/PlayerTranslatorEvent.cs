using QSB.Events;
using QSB.Messaging;

namespace QSB.Tools
{
    public class PlayerTranslatorEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.TranslatorActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener(EventNames.EquipTranslator, HandleEquip);
            GlobalMessenger.AddListener(EventNames.UnequipTranslator, HandleUnequip);
        }

        public override void CloseListener()
        {
            GlobalMessenger.RemoveListener(EventNames.EquipTranslator, HandleEquip);
            GlobalMessenger.RemoveListener(EventNames.UnequipTranslator, HandleUnequip);
        }

        private void HandleEquip() => SendEvent(CreateMessage(true));
        private void HandleUnequip() => SendEvent(CreateMessage(false));

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            AboutId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.AboutId);
            player.UpdateState(State.Translator, message.ToggleValue);
            player.Translator?.ChangeEquipState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Translator, message.ToggleValue);
        }
    }
}
