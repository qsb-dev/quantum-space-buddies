using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class SpinMessage : QSBWorldObjectMessage<QSBGhostController, TurnSpeed>
{
	public SpinMessage(TurnSpeed turnSpeed) : base(turnSpeed) { }

	public override void OnReceiveRemote()
	{
		WorldObject.Spin(Data, true);
	}
}
