using QSB.Messaging;

namespace QSB.Events
{
    class PlayerTranslatorEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.TranslatorActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("EquipTranslator", () => SendEvent(
                new ToggleMessage {
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    On = true
                }));
            GlobalMessenger.AddListener("UnequipTranslator", () => SendEvent(
                new ToggleMessage {
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    On = false
                }));
        }

        public override void OnReceive(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            var tool = player.Translator;
            player.UpdateState(State.Translator, message.On);
            if (message.On)
            {
                tool.EquipTool();
            }
            else
            {
                tool.UnequipTool();
            }
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            var tool = PlayerRegistry.LocalPlayer.Translator;
            if (message.On)
            {
                tool.EquipTool();
            }
            else
            {
                tool.UnequipTool();
            }
        }
    }
}
