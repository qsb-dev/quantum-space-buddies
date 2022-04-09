using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class PathfindLocalPositionMessage : QSBWorldObjectMessage<QSBGhostController, (int sectorId, Vector3 localPosition, float speed, float acceleration)>
{
	public PathfindLocalPositionMessage(Sector sector, Vector3 worldPosition, float speed, float acceleration) : base(Process(sector, worldPosition, speed, acceleration)) { }

	private static (int sectorId, Vector3 localPosition, float speed, float acceleration) Process(Sector sector, Vector3 worldPosition, float speed, float acceleration)
	{
		(int sectorId, Vector3 localPosition, float speed, float acceleration) ret = new();

		ret.speed = speed;
		ret.acceleration = acceleration;

		var qsbSector = sector.GetWorldObject<QSBSector>();
		ret.sectorId = qsbSector.ObjectId;
		ret.localPosition = qsbSector.AttachedObject.transform.InverseTransformPoint(worldPosition);

		return ret;
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole($"Error - Received PathfindLocalPositionMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		var sector = QSBWorldSync.GetWorldObject<QSBSector>(Data.sectorId);
		var worldPos = sector.AttachedObject.transform.TransformPoint(Data.localPosition);

		DebugLog.DebugWrite($"{WorldObject.AttachedObject.name} Pathfind to local position {WorldObject.AttachedObject.transform.InverseTransformPoint(worldPos)} with speed:{Data.speed}, acceleration:{Data.acceleration}");
		WorldObject.AttachedObject.PathfindToLocalPosition(WorldObject.AttachedObject.transform.InverseTransformPoint(worldPos), Data.speed, Data.acceleration);
	}
}
