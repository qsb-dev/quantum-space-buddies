using QSB.EventsCore;
using QSB.Player;

namespace QSB.Animation.Events
{
    public class AnimTriggerEvent : QSBEvent<AnimTriggerMessage>
    {
        public override EventType Type => EventType.AnimTrigger;

        public override void SetupListener() => GlobalMessenger<short, short, float>.AddListener(EventNames.QSBAnimTrigger, Handler);

        public override void CloseListener() => GlobalMessenger<short, short, float>.RemoveListener(EventNames.QSBAnimTrigger, Handler);

        private void Handler(short typeId, short triggerId, float value) => SendEvent(CreateMessage(typeId, triggerId, value));

        private AnimTriggerMessage CreateMessage(short typeId, short triggerId, float value) => new AnimTriggerMessage
        {
            AboutId = LocalPlayerId,
            TypeId = typeId,
            TriggerId = triggerId,
            Value = value
        };

        public override void OnReceiveRemote(AnimTriggerMessage message)
        {
            var animationSync = QSBPlayerManager.GetSyncObject<AnimationSync>(message.AboutId);
            if (animationSync == null)
            {
                return;
            }
            animationSync.SetAnimationType((AnimationType)message.TypeId);
            animationSync.HandleTrigger((AnimTrigger)message.TriggerId, message.Value);
        }
    }
}
