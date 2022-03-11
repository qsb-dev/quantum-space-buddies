using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.RaftSync.Messages;

public class RaftDockMessage : QSBWorldObjectMessage<QSBRaftDock, int>
{
	public RaftDockMessage(QSBRaft qsbRaft) : base(qsbRaft.ObjectId) { }
	public override void OnReceiveRemote() => WorldObject.Dock(Data.GetWorldObject<QSBRaft>());
}
