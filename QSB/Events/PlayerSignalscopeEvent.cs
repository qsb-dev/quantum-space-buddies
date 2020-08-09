using QSB.Messaging;

namespace QSB.Events
{
    class PlayerSignalscopeEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.SignalscopeActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", var => SendEvent(new ToggleMessage { On = true }));
            GlobalMessenger.AddListener("UnequipSignalscope", () => SendEvent(new ToggleMessage { On = false }));
        }

        public override void OnReceive(uint sender, ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(sender);
            var tool = player.Signalscope;
            player.UpdateState(State.Signalscope, message.On);
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
            var tool = PlayerRegistry.LocalPlayer.Signalscope;
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
