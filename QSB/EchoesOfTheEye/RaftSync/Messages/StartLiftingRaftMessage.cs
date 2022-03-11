using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.Messages;

public class StartLiftingRaftMessage : QSBMessage<int>
{
	public StartLiftingRaftMessage(QSBRaft qsbRaft) : base(qsbRaft.ObjectId) { }
	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;
	public override void OnReceiveRemote() => RaftManager.StartLiftingRaft(Data.GetWorldObject<QSBRaft>());
}
