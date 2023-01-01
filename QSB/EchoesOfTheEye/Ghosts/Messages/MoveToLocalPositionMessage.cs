using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class MoveToLocalPositionMessage : QSBWorldObjectMessage<QSBGhostController, (Vector3 localPosition, float speed, float acceleration)>
{
	public MoveToLocalPositionMessage(Vector3 localPos, float speed, float accel) : base((localPos, speed, accel)) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received MoveToLocalPosition on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		WorldObject.MoveToLocalPosition(Data.localPosition, Data.speed, Data.acceleration, true);
	}
}
