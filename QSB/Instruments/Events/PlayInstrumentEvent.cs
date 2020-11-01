using QSB.Events;
using QSB.Messaging;

namespace QSB.Instruments.Events
{
    public class PlayInstrumentEvent : QSBEvent<PlayInstrumentMessage>
    {
        public override EventType Type => EventType.FullStateRequest;

        public override void SetupListener() => GlobalMessenger<InstrumentType, bool>.AddListener(EventNames.QSBPlayerStatesRequest, Handler);

        public override void CloseListener() => GlobalMessenger<InstrumentType, bool>.RemoveListener(EventNames.QSBPlayerStatesRequest, Handler);

        private void Handler(InstrumentType type, bool state) => SendEvent(CreateMessage(type, state));

        private PlayInstrumentMessage CreateMessage(InstrumentType type, bool state) => new PlayInstrumentMessage
        {
            AboutId = LocalPlayerId,
            Type = type,
            State = state
        };
    }
}
