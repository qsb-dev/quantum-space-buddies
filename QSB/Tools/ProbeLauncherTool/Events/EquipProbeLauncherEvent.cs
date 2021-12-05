using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.Tools.ProbeLauncherTool.Events
{
	public class EquipProbeLauncherEvent : QSBEvent<ToggleMessage>
	{
		public override bool RequireWorldObjectsReady => false;

		private bool _nonPlayerLauncherEquipped;

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

		private void HandleEquip(ProbeLauncher var)
		{
			if (var != QSBPlayerManager.LocalPlayer.LocalProbeLauncher)
			{
				_nonPlayerLauncherEquipped = true;
				return;
			}

			if (_nonPlayerLauncherEquipped)
			{
				DebugLog.ToConsole($"Warning - Trying to equip player launcher whilst non player launcher is still equipped?", OWML.Common.MessageType.Warning);
				return;
			}

			SendEvent(CreateMessage(true));
		}

		private void HandleUnequip(ProbeLauncher var)
		{
			if (var != QSBPlayerManager.LocalPlayer.LocalProbeLauncher)
			{
				_nonPlayerLauncherEquipped = false;
				return;
			}

			if (_nonPlayerLauncherEquipped)
			{
				DebugLog.ToConsole($"Warning - Trying to de-equip player launcher whilst non player launcher is still equipped?", OWML.Common.MessageType.Warning);
				return;
			}

			SendEvent(CreateMessage(false));
		}

		private ToggleMessage CreateMessage(bool value) => new()
		{
			AboutId = LocalPlayerId,
			ToggleValue = value
		};

		public override void OnReceiveRemote(bool server, ToggleMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.ProbeLauncherEquipped = message.ToggleValue;
			player.ProbeLauncher?.ChangeEquipState(message.ToggleValue);
		}

		public override void OnReceiveLocal(bool server, ToggleMessage message) =>
			QSBPlayerManager.LocalPlayer.ProbeLauncherEquipped = message.ToggleValue;
	}
}