using QSB.Messaging;
using QSB.Utility;

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
                    ToggleValue = true
                }));
            GlobalMessenger<ProbeLauncher>.AddListener("ProbeLauncherUnequipped", var => SendEvent(
                new ToggleMessage {
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    ToggleValue = false
                }));
        }

        public override void OnReceive(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.ProbeLauncher, message.ToggleValue);
            player.ProbeLauncher.ChangeEquipState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.ProbeLauncher.ChangeEquipState(message.ToggleValue);
        }
    }
}
