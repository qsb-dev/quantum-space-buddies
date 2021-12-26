using QSB.Animation.NPC.WorldObjects;
using QSB.Messaging;

namespace QSB.Animation.NPC.Messages
{
	internal class NpcAnimationMessage : QSBBoolWorldObjectMessage<INpcAnimController>
	{
		public NpcAnimationMessage(bool start) => Value = start;

		public NpcAnimationMessage() { }

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
