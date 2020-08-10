using QSB.Messaging;
using QSB.Utility;

namespace QSB.Events
{
    class PlayerTranslatorEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.TranslatorActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("EquipTranslator", () => SendEvent(
                new ToggleMessage {
                    SenderId = LocalPlayerId,
                    ToggleValue = true
                }));
            GlobalMessenger.AddListener("UnequipTranslator", () => SendEvent(
                new ToggleMessage {
                    SenderId = LocalPlayerId,
                    ToggleValue = false
                }));
        }

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.Translator, message.ToggleValue);
            player.Translator?.ChangeEquipState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Translator, message.ToggleValue);
        }
    }
}
