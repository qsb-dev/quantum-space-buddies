using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Messages;

internal class PlayerRetrieveProbeMessage : QSBMessage<bool>
{
	public PlayerRetrieveProbeMessage(bool playEffects) => Data = playEffects;

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.ProbeLauncher.RetrieveProbe(Data);
	}
}