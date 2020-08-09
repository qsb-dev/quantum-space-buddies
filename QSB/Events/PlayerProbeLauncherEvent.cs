using QSB.Messaging;

namespace QSB.Events
{
    class PlayerProbeLauncherEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.ProbeLauncherActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherEquipped", var => SendEvent(
                new ToggleMessage {
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    On = true
                }));
            GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", var => SendEvent(
                new ToggleMessage {
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    On = false
                }));
        }

        public override void OnReceive(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            var tool = player.ProbeLauncher;
            player.UpdateState(State.ProbeLauncher, message.On);
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
            var tool = PlayerRegistry.LocalPlayer.ProbeLauncher;
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
