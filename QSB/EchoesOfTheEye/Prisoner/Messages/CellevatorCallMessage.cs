using QSB.EchoesOfTheEye.Prisoner.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

internal class CellevatorCallMessage : QSBWorldObjectMessage<QSBPrisonCellElevator, int>
{
	public CellevatorCallMessage(int index) : base(index) { }

	public override void OnReceiveRemote()
	{
		WorldObject.CallToFloorIndex(Data, true);
	}
}
