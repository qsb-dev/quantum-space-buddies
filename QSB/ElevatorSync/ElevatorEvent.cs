using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.ElevatorSync
{
    public class ElevatorEvent : QSBEvent<ElevatorMessage>
    {
        public override MessageType Type => MessageType.Elevator;

        public override void SetupListener()
        {
            GlobalMessenger<ElevatorDirection>.AddListener(EventNames.QSBStartLift, direction => SendEvent(CreateMessage(direction)));
        }

        private ElevatorMessage CreateMessage(ElevatorDirection direction) => new ElevatorMessage
        {
            SenderId = PlayerRegistry.LocalPlayer.NetId,
            Direction = direction
        };

        public override void OnReceiveRemote(ElevatorMessage message)
        {
            if (!IsInUniverse || message.SenderId == PlayerRegistry.LocalPlayer.NetId)
            {
                return;
            }
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            DebugLog.ToAll($"{player.Name} called the elevator {message.Direction}");
            ElevatorController.Instance.RemoteCall(message.Direction);
        }
    }
}
