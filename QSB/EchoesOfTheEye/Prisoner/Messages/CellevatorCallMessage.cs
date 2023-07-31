using QSB.EchoesOfTheEye.Prisoner.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

public class CellevatorCallMessage : QSBWorldObjectMessage<QSBPrisonCellElevator, int>
{
	public CellevatorCallMessage(int floorIndex) : base(floorIndex) { }
	public override void OnReceiveRemote() => WorldObject.AttachedObject.CallElevatorToFloor(Data);
}
