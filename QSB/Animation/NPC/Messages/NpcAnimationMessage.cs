using QSB.Animation.NPC.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Animation.NPC.Messages
{
	internal class NpcAnimationMessage : QSBBoolWorldObjectMessage<INpcAnimController>
	{
		public NpcAnimationMessage(bool start) => Value = start;

		public NpcAnimationMessage() { }

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			if (Value)
			{
				WorldObject.StartConversation();
			}
			else
			{
				WorldObject.EndConversation();
			}
		}
	}
}
