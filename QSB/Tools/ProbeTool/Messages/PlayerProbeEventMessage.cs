using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.Tools.ProbeTool.Messages;

public class PlayerProbeEventMessage : QSBMessage<ProbeEvent>
{
	public PlayerProbeEventMessage(ProbeEvent probeEvent) : base(probeEvent) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		if (!player.IsReady || player.Probe == null)
		{
			return;
		}

		var probe = player.Probe;
		probe.HandleEvent(Data);
	}
}