using QSB.Events;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.OrbSync
{
    public class OrbSlotEvent : QSBEvent<BoolWorldObjectMessage>
    {
        public override EventType Type => EventType.OrbSlot;

        public override void SetupListener() => GlobalMessenger<int, bool>.AddListener(EventNames.QSBOrbSlot, Handler);

        public override void CloseListener() => GlobalMessenger<int, bool>.RemoveListener(EventNames.QSBOrbSlot, Handler);

        private void Handler(int id, bool state) => SendEvent(CreateMessage(id, state));

        private BoolWorldObjectMessage CreateMessage(int id, bool state) => new BoolWorldObjectMessage
        {
            AboutId = LocalPlayerId,
            ObjectId = id,
            State = state
        };

        public override void OnReceiveRemote(BoolWorldObjectMessage message)
        {
            var orbSlot = WorldRegistry.GetObject<QSBOrbSlot>(message.ObjectId);
            orbSlot?.SetState(message.State);
        }
    }
}
