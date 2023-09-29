using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AirlockSync.Messages;

public class AirlockInitialStateMessage : QSBWorldObjectMessage<QSBGhostAirlock, (bool innerDoorOpen, bool outerDoorOpen, bool pressurized)>
{
	public AirlockInitialStateMessage(bool innerDoorOpen, bool outerDoorOpen, bool pressurized) : base((innerDoorOpen, outerDoorOpen, pressurized)) { }

	public override void OnReceiveRemote()
	{
		var airlock = WorldObject.AttachedObject;
		airlock._innerDoor.SetOpenImmediate(Data.innerDoorOpen);
		airlock._outerDoor.SetOpenImmediate(Data.outerDoorOpen);
		airlock.SetPressurization(Data.pressurized);
	}
}
