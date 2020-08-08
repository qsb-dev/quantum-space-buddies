namespace QSB.Events
{
    class PlayerSignalscopeEvent : QSBEvent
    {
        public override EventType Type => EventType.SignalscopeActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", var => EventSender.SendEvent(this, PlayerRegistry.LocalPlayer.NetId, true));
            GlobalMessenger.AddListener("UnequipSignalscope", () => EventSender.SendEvent(this, PlayerRegistry.LocalPlayer.NetId, false));
        }

        public override void OnReceive(uint sender, object[] data)
        {
            var player = PlayerRegistry.GetPlayer(sender);
            var tool = player.Signalscope;
            player.UpdateState(State.Signalscope, (bool)data[0]);
            if ((bool)data[0] == true)
            {
                tool.EquipTool();
            }
            else
            {
                tool.UnequipTool();
            }
        }

        public override void OnReceiveLocal(object[] data)
        {
            var tool = PlayerRegistry.LocalPlayer.Signalscope;
            if ((bool)data[0] == true)
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
