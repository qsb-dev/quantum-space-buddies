using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using System;

namespace QSB.Instruments.Events
{
    public class PlayInstrumentEvent : QSBEvent<PlayInstrumentMessage>
    {
        public override EventType Type => EventType.PlayInstrument;

        public override void SetupListener() => GlobalMessenger<InstrumentType>.AddListener(EventNames.QSBPlayInstrument, Handler);

        public override void CloseListener() => GlobalMessenger<InstrumentType>.RemoveListener(EventNames.QSBPlayInstrument, Handler);

        private void Handler(InstrumentType type) => SendEvent(CreateMessage(type));

        private PlayInstrumentMessage CreateMessage(InstrumentType type) => new PlayInstrumentMessage
        {
            AboutId = LocalPlayerId,
            Type = type
        };

        public override void OnReceiveRemote(PlayInstrumentMessage message)
        {
            PlayerRegistry.GetPlayer(message.AboutId).CurrentInstrument = message.Type;
            DebugLog.DebugWrite($"Player ID {message.AboutId} now playing instrument {Enum.GetName(typeof(InstrumentType), message.Type)}");
        }
    }
}
