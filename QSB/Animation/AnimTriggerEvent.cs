using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Animation
{
    public class AnimTriggerEvent : QSBEvent<AnimTriggerMessage>
    {
        public override MessageType Type => MessageType.PlayerJoin;

        public override void SetupListener()
        {
            GlobalMessenger<short, float>.AddListener(EventNames.QSBAnimTrigger, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<short, float>.RemoveListener(EventNames.QSBAnimTrigger, Handler);
        }

        private void Handler(short triggerId, float value) => SendEvent(CreateMessage(triggerId, value));

        private AnimTriggerMessage CreateMessage(short triggerId, float value) => new AnimTriggerMessage
        {
            AboutId = LocalPlayerId,
            TriggerId = triggerId,
            Value = value
        };

        public override void OnReceiveRemote(AnimTriggerMessage message)
        {
            var animationSync = PlayerRegistry.GetSyncObject<AnimationSync>(message.AboutId);
            if (animationSync == null)
            {
                return;
            }
            animationSync.HandleTrigger((AnimTrigger)message.TriggerId, message.Value);
        }
    }
}
