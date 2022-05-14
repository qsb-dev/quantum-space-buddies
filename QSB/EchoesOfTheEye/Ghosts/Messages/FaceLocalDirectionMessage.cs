using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class FaceLocalDirectionMessage : QSBWorldObjectMessage<QSBGhostController, (Vector3 localDirection, float degreesPerSecond, float turnAcceleration)>
{
	public FaceLocalDirectionMessage(Vector3 localDirection, float degreesPerSecond, float turnAcceleration) : base((localDirection, degreesPerSecond, turnAcceleration)) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received FaceLocalDirectionMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		WorldObject.FaceLocalDirection(Data.localDirection, Data.degreesPerSecond, Data.turnAcceleration, true);
	}
}
