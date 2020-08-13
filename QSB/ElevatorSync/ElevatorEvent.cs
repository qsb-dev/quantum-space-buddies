using QSB.Events;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ElevatorSync
{
    public class ElevatorEvent : QSBEvent<ElevatorMessage>
    {
        public override MessageType Type => MessageType.Elevator;

        public override void SetupListener()
        {
            GlobalMessenger<int, ElevatorDirection>.AddListener(EventNames.QSBStartLift, (id, direction) => SendEvent(CreateMessage(id, direction)));
        }

        private ElevatorMessage CreateMessage(int id, ElevatorDirection direction) => new ElevatorMessage
        {
            SenderId = PlayerRegistry.LocalPlayer.NetId,
            Direction = direction,
            Id = id
        };

        public override void OnReceiveRemote(ElevatorMessage message)
        {
            if (!IsInUniverse)
            {
                return;
            }
            WorldRegistry.GetObject<QSBElevator>(message.Id).RemoteCall(message.Direction);
        }
    }
}
