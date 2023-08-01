using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Messages;

public class PlayerLauncherChangeModeMessage : QSBMessage
{
	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.ProbeLauncherTool.ChangeMode();
	}
}
