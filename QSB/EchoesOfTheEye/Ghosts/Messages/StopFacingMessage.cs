using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class StopFacingMessage : QSBWorldObjectMessage<QSBGhostController>
{
	public override void OnReceiveRemote()
	{
		WorldObject.StopFacing(true);
	}
}
