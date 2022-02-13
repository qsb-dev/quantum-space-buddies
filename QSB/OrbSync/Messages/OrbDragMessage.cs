using QSB.AuthoritySync;
using QSB.Messaging;
using QSB.OrbSync.WorldObjects;

namespace QSB.OrbSync.Messages
{
	public class OrbDragMessage : QSBWorldObjectMessage<QSBOrb, bool>
	{
		public OrbDragMessage(bool isDragging) => Value = isDragging;

		public override void OnReceiveLocal()
		{
			if (QSBCore.IsHost && Value)
			{
				WorldObject.TransformSync.netIdentity.ServerUpdateAuthQueue(From, AuthQueueAction.Force);
			}
		}

		public override void OnReceiveRemote()
		{
			OnReceiveLocal();
			WorldObject.SetDragging(Value);
		}
	}
}