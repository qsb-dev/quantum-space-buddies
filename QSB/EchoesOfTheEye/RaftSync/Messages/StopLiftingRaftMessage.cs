using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.Messages;

public class StopLiftingRaftMessage : QSBMessage<bool>
{
	public StopLiftingRaftMessage(bool damBroken) : base(damBroken) { }
	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;
	public override void OnReceiveRemote() => RaftManager.StopLiftingRaft(Data);
}
