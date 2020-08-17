using QSB.Events;
using QSB.Messaging;

namespace QSB.Tools
{
    public class PlayerProbeLauncherEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.ProbeLauncherActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<ProbeLauncher>.AddListener(EventNames.ProbeLauncherEquipped, HandleEquip);
            GlobalMessenger<ProbeLauncher>.AddListener(EventNames.ProbeLauncherUnequipped, HandleUnequip);
        }

        public override void CloseListener()
        {
            GlobalMessenger<ProbeLauncher>.RemoveListener(EventNames.ProbeLauncherEquipped, HandleEquip);
            GlobalMessenger<ProbeLauncher>.RemoveListener(EventNames.ProbeLauncherUnequipped, HandleUnequip);
        }

        private void HandleEquip(ProbeLauncher var) => SendEvent(CreateMessage(true));
        private void HandleUnequip(ProbeLauncher var) => SendEvent(CreateMessage(false));

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            AboutId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.AboutId);
            player?.UpdateState(State.ProbeLauncher, message.ToggleValue);
            if (!QSBSceneManager.IsInUniverse)
            {
                return;
            }
            player?.ProbeLauncher?.ChangeEquipState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.ProbeLauncher, message.ToggleValue);
        }
    }
}
