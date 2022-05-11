using QSB.EchoesOfTheEye.Prisoner.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

internal class CellevatorCallMessage : QSBWorldObjectMessage<QSBPrisonCellElevator, int>
{
	public CellevatorCallMessage(int floorIndex) : base(floorIndex) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.CallElevatorToFloor(Data));
}
