using QSB.EventsCore;
using QSB.Player;

namespace QSB.Animation.Events
{
    public class ChangeAnimTypeEvent : QSBEvent<ChangeAnimTypeMessage>
    {
        public override EventType Type => EventType.PlayInstrument;

        public override void SetupListener() => GlobalMessenger<AnimationType>.AddListener(EventNames.QSBChangeAnimType, Handler);

        public override void CloseListener() => GlobalMessenger<AnimationType>.RemoveListener(EventNames.QSBChangeAnimType, Handler);

        private void Handler(AnimationType type) => SendEvent(CreateMessage(type));

        private ChangeAnimTypeMessage CreateMessage(AnimationType type) => new ChangeAnimTypeMessage
        {
            AboutId = LocalPlayerId,
            Type = type
        };

        public override void OnReceiveRemote(ChangeAnimTypeMessage message)
        {
            QSBPlayerManager.GetPlayer(message.AboutId).Animator.SetAnimationType(message.Type);
        }
    }
}