using QSB.EventsCore;
using QSB.Instruments;
using QSB.MessagesCore;
using QSB.Player;

namespace QSB.Animation.Events
{
    public class ChangeAnimTypeEvent : QSBEvent<EnumMessage<AnimationType>>
    {
        public override EventType Type => EventType.PlayInstrument;

        public override void SetupListener() => GlobalMessenger<uint, AnimationType>.AddListener(EventNames.QSBChangeAnimType, Handler);

        public override void CloseListener() => GlobalMessenger<uint, AnimationType>.RemoveListener(EventNames.QSBChangeAnimType, Handler);

        private void Handler(uint player, AnimationType type) => SendEvent(CreateMessage(player, type));

        private EnumMessage<AnimationType> CreateMessage(uint player, AnimationType type) => new EnumMessage<AnimationType>
        {
            AboutId = player,
            Value = type
        };

        public override void OnReceiveRemote(EnumMessage<AnimationType> message)
        {
            QSBPlayerManager.GetPlayer(message.AboutId).AnimationSync.SetAnimationType(message.Value);
            QSBPlayerManager.GetSyncObject<InstrumentsManager>(message.AboutId).CheckInstrumentProps(message.Value);
        }
    }
}