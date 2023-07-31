using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Messages;

public class PlayerRetrieveProbeMessage : QSBMessage<bool>
{
	public PlayerRetrieveProbeMessage(bool playEffects) : base(playEffects) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.ProbeLauncherTool.RetrieveProbe(Data);
	}
}