using QSB.EventsCore;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ElevatorSync.Events
{
    public class ElevatorEvent : QSBEvent<BoolWorldObjectMessage>
    {
        public override EventType Type => EventType.Elevator;

        public override void SetupListener() => GlobalMessenger<int, bool>.AddListener(EventNames.QSBStartLift, Handler);

        public override void CloseListener() => GlobalMessenger<int, bool>.RemoveListener(EventNames.QSBStartLift, Handler);

        private void Handler(int id, bool direction) => SendEvent(CreateMessage(id, direction));

        private BoolWorldObjectMessage CreateMessage(int id, bool direction) => new BoolWorldObjectMessage
        {
            State = direction,
            ObjectId = id
        };

        public override void OnReceiveRemote(BoolWorldObjectMessage message)
        {
            var elevator = WorldRegistry.GetObject<QSBElevator>(message.ObjectId);
            elevator?.RemoteCall(message.State);
        }
    }
}
