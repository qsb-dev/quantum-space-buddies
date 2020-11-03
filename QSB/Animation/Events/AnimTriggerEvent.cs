using QSB.EventsCore;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Animation.Events
{
    public class AnimTriggerEvent : QSBEvent<AnimTriggerMessage>
    {
        public override EventType Type => EventType.AnimTrigger;

        public override void SetupListener() => GlobalMessenger<short, float>.AddListener(EventNames.QSBAnimTrigger, Handler);

        public override void CloseListener() => GlobalMessenger<short, float>.RemoveListener(EventNames.QSBAnimTrigger, Handler);

        private void Handler(short triggerId, float value) => SendEvent(CreateMessage(triggerId, value));

        private AnimTriggerMessage CreateMessage(short triggerId, float value) => new AnimTriggerMessage
        {
            AboutId = LocalPlayerId,
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
            animationSync.HandleTrigger((AnimTrigger)message.TriggerId, message.Value);
        }
    }
}
