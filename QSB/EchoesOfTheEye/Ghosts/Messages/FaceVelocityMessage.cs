using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class FaceVelocityMessage : QSBWorldObjectMessage<QSBGhostController>
{
	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received FaceVelocityMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		WorldObject.FaceVelocity(true);
	}
}
