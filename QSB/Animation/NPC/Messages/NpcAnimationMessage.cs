using QSB.Animation.NPC.WorldObjects;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Animation.NPC.Messages
{
	internal class NpcAnimationMessage : QSBEnumWorldObjectMessage<INpcAnimController, AnimationEvent>
	{
		public NpcAnimationMessage(AnimationEvent animationEvent) => Value = animationEvent;

		public NpcAnimationMessage() { }

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			switch (Value)
			{
				case AnimationEvent.StartConversation:
					WorldObject.StartConversation();
					break;
				case AnimationEvent.EndConversation:
					WorldObject.EndConversation();
					break;
			}
		}
	}
}
