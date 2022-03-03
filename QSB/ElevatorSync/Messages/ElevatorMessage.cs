using QSB.ElevatorSync.WorldObjects;
using QSB.Messaging;

namespace QSB.ElevatorSync.Messages
{
	public class ElevatorMessage : QSBWorldObjectMessage<QSBElevator, bool>
	{
		public ElevatorMessage(bool isGoingUp) => Data = isGoingUp;

		public override void OnReceiveRemote() => WorldObject.RemoteCall(Data);
	}
}