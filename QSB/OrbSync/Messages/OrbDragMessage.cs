using QSB.AuthoritySync;
using QSB.Messaging;
using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.OrbSync.Messages
{
	public class OrbDragMessage : QSBBoolWorldObjectMessage<QSBOrb>
	{
		public OrbDragMessage(bool isDragging) => Value = isDragging;

		public override void OnReceiveLocal()
		{
			var qsbOrb = ObjectId.GetWorldObject<QSBOrb>();

			if (QSBCore.IsHost && Value)
			{
				qsbOrb.TransformSync.netIdentity.UpdateAuthQueue(From, AuthQueueAction.Force);
			}
		}

		public override void OnReceiveRemote()
		{
			var qsbOrb = ObjectId.GetWorldObject<QSBOrb>();

			if (QSBCore.IsHost && Value)
			{
				qsbOrb.TransformSync.netIdentity.UpdateAuthQueue(From, AuthQueueAction.Force);
			}

			qsbOrb.SetDragging(Value);
		}
	}
}