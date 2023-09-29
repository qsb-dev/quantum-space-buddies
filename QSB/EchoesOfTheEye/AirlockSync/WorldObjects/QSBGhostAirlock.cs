using QSB.EchoesOfTheEye.AirlockSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AirlockSync.WorldObjects;

public class QSBGhostAirlock : WorldObject<GhostAirlock>
{
	public override void SendInitialState(uint to)
		=> this.SendMessage(
			new AirlockInitialStateMessage(
				AttachedObject._innerDoor.IsOpen(),
				AttachedObject._outerDoor.IsOpen(),
				AttachedObject._pressurized
			)
		);
}
