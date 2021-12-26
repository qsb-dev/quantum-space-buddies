using OWML.Common;
using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;

namespace QSB.Tools.ProbeLauncherTool.Messages
{
	public class EquipProbeLauncherMessage : QSBBoolMessage
	{
		static EquipProbeLauncherMessage()
		{
			GlobalMessenger<ProbeLauncher>.AddListener(EventNames.ProbeLauncherEquipped, launcher => Handle(launcher, true));
			GlobalMessenger<ProbeLauncher>.AddListener(EventNames.ProbeLauncherUnequipped, launcher => Handle(launcher, false));
		}

		private static bool _nonPlayerLauncherEquipped;

		private static void Handle(ProbeLauncher launcher, bool equipped)
		{
			if (PlayerTransformSync.LocalInstance)
			{
				if (launcher != QSBPlayerManager.LocalPlayer.LocalProbeLauncher)
				{
					_nonPlayerLauncherEquipped = equipped;
					return;
				}

				if (_nonPlayerLauncherEquipped)
				{
					DebugLog.ToConsole($"Warning - Trying to equip/unequip player launcher whilst non player launcher is still equipped?", MessageType.Warning);
					return;
				}

				new EquipProbeLauncherMessage(equipped).Send();
			}
		}

		private EquipProbeLauncherMessage(bool equipped) => Value = equipped;

		public EquipProbeLauncherMessage() { }

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.ProbeLauncherEquipped = Value;
			player.ProbeLauncher?.ChangeEquipState(Value);
		}

		public override void OnReceiveLocal() =>
			QSBPlayerManager.LocalPlayer.ProbeLauncherEquipped = Value;
	}
}