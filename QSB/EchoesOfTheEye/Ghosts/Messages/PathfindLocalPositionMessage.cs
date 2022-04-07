using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class PathfindLocalPositionMessage : QSBWorldObjectMessage<QSBGhostController, (Vector3 worldPos, MoveType moveType)>
{
	public PathfindLocalPositionMessage(Vector3 worldPosition, MoveType moveType) : base((worldPosition, moveType)) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole($"Error - Received PathfindLocalPositionMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		DebugLog.DebugWrite($"{WorldObject.AttachedObject.name} Pathfind to local position {WorldObject.AttachedObject.WorldToLocalPosition(Data.worldPos)} with movetype {Data.moveType}");
		WorldObject.AttachedObject.PathfindToLocalPosition(WorldObject.AttachedObject.WorldToLocalPosition(Data.worldPos), Data.moveType);
	}
}
