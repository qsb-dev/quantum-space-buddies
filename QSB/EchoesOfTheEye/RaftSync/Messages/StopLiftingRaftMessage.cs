using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.Messages;

public class StopLiftingRaftMessage : QSBMessage
{
	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		// todo
	}
}
