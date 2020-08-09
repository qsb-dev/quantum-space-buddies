using QSB.Messaging;

namespace QSB.Events
{
    class PlayerProbeLauncherEvent : QSBEvent
    {
        public override MessageType Type => MessageType.ProbeLauncherActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherEquipped", var => SendEvent(PlayerRegistry.LocalPlayer.NetId, true));
            GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", var => SendEvent(PlayerRegistry.LocalPlayer.NetId, false));
        }

        public override void OnReceive(uint sender, object[] data)
        {
            var player = PlayerRegistry.GetPlayer(sender);
            var tool = player.ProbeLauncher;
            player.UpdateState(State.ProbeLauncher, (bool)data[0]);
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
            var tool = PlayerRegistry.LocalPlayer.ProbeLauncher;
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
