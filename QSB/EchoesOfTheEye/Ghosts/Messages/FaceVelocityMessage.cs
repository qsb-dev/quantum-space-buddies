using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class FaceVelocityMessage : QSBWorldObjectMessage<QSBGhostController>
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
