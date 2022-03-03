using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility;

namespace QSB.Tools.ProbeLauncherTool.Messages
{
	public class EquipProbeLauncherMessage : QSBMessage<bool>
	{
		static EquipProbeLauncherMessage()
		{
			GlobalMessenger<ProbeLauncher>.AddListener(OWEvents.ProbeLauncherEquipped, launcher => Handle(launcher, true));
			GlobalMessenger<ProbeLauncher>.AddListener(OWEvents.ProbeLauncherUnequipped, launcher => Handle(launcher, false));
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

		private EquipProbeLauncherMessage(bool equipped) => Data = equipped;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.ProbeLauncherEquipped = Data;
			player.ProbeLauncher?.ChangeEquipState(Data);
		}

		public override void OnReceiveLocal() =>
			QSBPlayerManager.LocalPlayer.ProbeLauncherEquipped = Data;
	}
}