using QSB.Events;
using QSB.Messaging;

namespace QSB.Tools
{
    public class PlayerProbeLauncherEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.ProbeLauncherActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<ProbeLauncher>.AddListener(EventNames.ProbeLauncherEquipped, var => SendEvent(CreateMessage(true)));
            GlobalMessenger<ProbeLauncher>.AddListener(EventNames.ProbeLauncherUnequipped, var => SendEvent(CreateMessage(false)));
        }

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            SenderId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.ProbeLauncher, message.ToggleValue);
            player.ProbeLauncher?.ChangeEquipState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.ProbeLauncher, message.ToggleValue);
        }
    }
}
