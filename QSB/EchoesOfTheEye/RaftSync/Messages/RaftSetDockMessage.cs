using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.Messages;

public class RaftSetDockMessage : QSBWorldObjectMessage<QSBRaft, int>
{
	public RaftSetDockMessage(RaftDock raftDock) :
		base(raftDock != null ? raftDock.GetWorldObject<QSBRaftDock>().ObjectId : -1) { }

	public override void OnReceiveRemote() =>
		WorldObject.SetDock(Data != -1 ? Data.GetWorldObject<QSBRaftDock>() : null).Forget();
}
