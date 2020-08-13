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
            GlobalMessenger<ElevatorDirection, string>.AddListener(EventNames.QSBStartLift, (direction, elevatorName) => SendEvent(CreateMessage(direction, elevatorName)));
        }

        private ElevatorMessage CreateMessage(ElevatorDirection direction, string elevatorName) => new ElevatorMessage
        {
            SenderId = PlayerRegistry.LocalPlayer.NetId,
            Direction = direction,
            UniqueName = elevatorName
        };

        public override void OnReceiveRemote(ElevatorMessage message)
        {
            if (!IsInUniverse)
            {
                return;
            }
            WorldRegistry.GetObject<QSBElevator>(message.UniqueName).RemoteCall(message.Direction);
        }
    }
}
