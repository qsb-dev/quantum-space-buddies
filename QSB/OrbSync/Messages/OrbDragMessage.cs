using QSB.AuthoritySync;
using QSB.Messaging;
using QSB.OrbSync.WorldObjects;

namespace QSB.OrbSync.Messages
{
	public class OrbDragMessage : QSBBoolWorldObjectMessage<QSBOrb>
	{
		public OrbDragMessage(bool isDragging) => Value = isDragging;

		public override void OnReceiveLocal()
		{
			if (QSBCore.IsHost && Value)
			{
				WorldObject.TransformSync.netIdentity.UpdateAuthQueue(From, AuthQueueAction.Force);
			}
		}

		public override void OnReceiveRemote()
		{
			OnReceiveLocal();
			WorldObject.SetDragging(Value);
		}
	}
}