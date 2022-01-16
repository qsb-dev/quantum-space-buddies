using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Messages
{
	internal class PlayerLaunchProbeMessage : QSBMessage
	{
		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			player.ProbeLauncher.LaunchProbe();
		}
	}
}
