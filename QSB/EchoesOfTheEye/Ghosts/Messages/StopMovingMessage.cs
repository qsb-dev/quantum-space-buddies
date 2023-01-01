using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class StopMovingMessage : QSBWorldObjectMessage<QSBGhostController, bool>
{
	public StopMovingMessage(bool instant) : base(instant) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received StopMovingMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		if (Data)
		{
			WorldObject.AttachedObject._velocity = Vector3.zero;
			// rest of code gets handled by the second StopMovingMessage
			return;
		}

		WorldObject.AttachedObject._moveToTargetPosition = false;
		WorldObject.AttachedObject._followNodePath = false;
		WorldObject.AttachedObject._hasFinalPathPosition = false;
	}
}
