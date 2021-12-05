using QSB.Animation.NPC.WorldObjects;
using QSB.Events;
using QSB.WorldSync;

namespace QSB.Animation.NPC.Events
{
	internal class NpcAnimationEvent : QSBEvent<NpcAnimationMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<AnimationEvent, int>.AddListener(EventNames.QSBNpcAnimEvent, Handler);
		public override void CloseListener() => GlobalMessenger<AnimationEvent, int>.RemoveListener(EventNames.QSBNpcAnimEvent, Handler);

		private void Handler(AnimationEvent animEvent, int index) => SendEvent(CreateMessage(animEvent, index));

		private NpcAnimationMessage CreateMessage(AnimationEvent animEvent, int index) => new()
		{
			AboutId = LocalPlayerId,
			AnimationEvent = animEvent,
			AnimControllerIndex = index
		};

		public override void OnReceiveRemote(bool server, NpcAnimationMessage message)
		{
			var qsbObj = QSBWorldSync.GetWorldFromId<INpcAnimController>(message.AnimControllerIndex);
			switch (message.AnimationEvent)
			{
				case AnimationEvent.StartConversation:
					qsbObj.StartConversation();
					break;
				case AnimationEvent.EndConversation:
					qsbObj.EndConversation();
					break;
			}
		}
	}
}
