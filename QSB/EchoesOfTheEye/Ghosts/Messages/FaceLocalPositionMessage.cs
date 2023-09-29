using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class FaceLocalPositionMessage : QSBWorldObjectMessage<QSBGhostController, (Vector3 localPosition, float degreesPerSecond, float turnAcceleration)>
{
	public FaceLocalPositionMessage(Vector3 localPos, float degPerSecond, float turnAccel) : base((localPos, degPerSecond, turnAccel)) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received FaceLocalPositionMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		WorldObject.FaceLocalPosition(Data.localPosition, Data.degreesPerSecond, Data.turnAcceleration, true);
	}
}
