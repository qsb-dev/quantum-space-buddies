using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class PathfindLocalPositionMessage : QSBWorldObjectMessage<QSBGhostController,
	(Vector3 localPosition, float speed, float acceleration)>
{
	public PathfindLocalPositionMessage(Vector3 localPosition, float speed, float acceleration) :
		base((localPosition, speed, acceleration)) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received PathfindLocalPositionMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		DebugLog.DebugWrite($"{WorldObject.AttachedObject.name} Pathfind to local position {Data.localPosition} with speed:{Data.speed}, acceleration:{Data.acceleration}");
		WorldObject.AttachedObject.PathfindToLocalPosition(Data.localPosition, Data.speed, Data.acceleration);
	}
}
