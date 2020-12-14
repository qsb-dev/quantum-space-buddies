using QSB.Events;
using QSB.Messages;
using QSB.Player;

namespace QSB.Tools.Events
{
	public class PlayerProbeLauncherEvent : QSBEvent<ToggleMessage>
	{
		public override EventType Type => EventType.ProbeLauncherActiveChange;

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

		public override void OnReceiveRemote(bool server, ToggleMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.UpdateState(State.ProbeLauncher, message.ToggleValue);
			player.ProbeLauncher?.ChangeEquipState(message.ToggleValue);
		}

		public override void OnReceiveLocal(bool server, ToggleMessage message)
		{
			QSBPlayerManager.LocalPlayer.UpdateState(State.ProbeLauncher, message.ToggleValue);
		}
	}
}