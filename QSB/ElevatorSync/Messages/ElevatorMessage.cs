using QSB.ElevatorSync.WorldObjects;
using QSB.Messaging;

namespace QSB.ElevatorSync.Messages
{
	public class ElevatorMessage : QSBBoolWorldObjectMessage<QSBElevator>
	{
		public ElevatorMessage(bool isGoingUp) => Value = isGoingUp;

		public ElevatorMessage() { }

		public override void OnReceiveRemote() => WorldObject.RemoteCall(Value);
	}
}