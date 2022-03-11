using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.Messages;

public class DockRaftMessage : QSBWorldObjectMessage<QSBRaftDock, int>
{
	public DockRaftMessage(RaftController raft) :
		base(raft.GetWorldObject<QSBRaft>().ObjectId) { }

	public override void OnReceiveRemote() =>
		WorldObject.Dock(Data.GetWorldObject<QSBRaft>().AttachedObject);
}
