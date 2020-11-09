using QSB.EventsCore;
using QSB.Player;
using QSB.Utility;

namespace QSB.Animation.Events
{
    public class ChangeAnimTypeEvent : QSBEvent<ChangeAnimTypeMessage>
    {
        public override EventType Type => EventType.PlayInstrument;

        public override void SetupListener() => GlobalMessenger<uint, AnimationType>.AddListener(EventNames.QSBChangeAnimType, Handler);

        public override void CloseListener() => GlobalMessenger<uint, AnimationType>.RemoveListener(EventNames.QSBChangeAnimType, Handler);

        private void Handler(uint player, AnimationType type) => SendEvent(CreateMessage(player, type));

        private ChangeAnimTypeMessage CreateMessage(uint player, AnimationType type) => new ChangeAnimTypeMessage
        {
            AboutId = player,
            Type = type
        };

        public override void OnReceiveRemote(ChangeAnimTypeMessage message)
        {
            DebugLog.DebugWrite($"ChangeAnimType for {message.AboutId} - {message.Type}");
            QSBPlayerManager.GetPlayer(message.AboutId).Animator.SetAnimationType(message.Type);
        }
    }
}