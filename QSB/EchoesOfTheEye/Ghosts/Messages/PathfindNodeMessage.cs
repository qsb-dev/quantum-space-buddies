using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class PathfindNodeMessage : QSBWorldObjectMessage<QSBGhostController, (int mapId, int nodeIndex, float speed, float acceleration)>
{
	public PathfindNodeMessage(GhostNode node, float speed, float acceleration) : base(Process(node, speed, acceleration)) { }

	private static (int mapId, int nodeIndex, float speed, float acceleration) Process(GhostNode node, float speed, float acceleration)
	{
		(int mapId, int nodeId, float speed, float acceleration) ret = new();

		ret.speed = speed;
		ret.acceleration = acceleration;

		var nodeMaps = QSBWorldSync.GetWorldObjects<QSBGhostNodeMap>();
		var owner = nodeMaps.First(x => x.AttachedObject._nodes.Contains(node));

		ret.mapId = owner.ObjectId;
		ret.nodeId = Array.IndexOf(owner.AttachedObject._nodes, node);

		return ret;
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received PathfindNodeMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		var map = Data.mapId.GetWorldObject<QSBGhostNodeMap>();
		var node = map.AttachedObject._nodes[Data.nodeIndex];

		WorldObject.PathfindToNode(node, Data.speed, Data.acceleration, true);
	}
}
