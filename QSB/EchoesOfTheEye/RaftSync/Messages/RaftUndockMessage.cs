using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.RaftSync.Messages
{
	public class RaftUndockMessage : QSBWorldObjectMessage<QSBRaftDock>
	{
		public override void OnReceiveRemote() => WorldObject.UndockFromRaftDock();
	}
}
