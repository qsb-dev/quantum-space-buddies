namespace QSB.Events
{
    class PlayerTrasnlatorEvent : QSBEvent
    {
        public override EventType Type => EventType.TranslatorActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("EquipTranslator", () => EventSender.SendEvent(this, PlayerRegistry.LocalPlayer.NetId, true));
            GlobalMessenger.AddListener("UnequipTranslator", () => EventSender.SendEvent(this, PlayerRegistry.LocalPlayer.NetId, false));
        }

        public override void OnReceive(uint sender, object[] data)
        {
            var player = PlayerRegistry.GetPlayer(sender);
            var tool = player.Translator;
            player.UpdateState(State.Translator, (bool)data[0]);
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
            var tool = PlayerRegistry.LocalPlayer.Translator;
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
