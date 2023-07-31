using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class FaceNodeMessage : QSBWorldObjectMessage<QSBGhostController, (int mapId, int nodeIndex, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern)>
{
	public FaceNodeMessage(GhostNode node, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern) : base(Process(node, turnSpeed, nodeDelay, autoFocusLantern)) { }

	private static (int mapId, int nodeIndex, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern) Process(GhostNode node, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern)
	{
		(int mapId, int nodeIndex, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern) ret = new();

		ret.turnSpeed = turnSpeed;
		ret.nodeDelay = nodeDelay;
		ret.autoFocusLantern = autoFocusLantern;

		var nodeMaps = QSBWorldSync.GetWorldObjects<QSBGhostNodeMap>();
		var owner = nodeMaps.First(x => x.AttachedObject._nodes.Contains(node));

		ret.mapId = owner.ObjectId;
		ret.nodeIndex = Array.IndexOf(owner.AttachedObject._nodes, node);

		return ret;
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received FaceNodeMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		var map = Data.mapId.GetWorldObject<QSBGhostNodeMap>();
		var node = map.AttachedObject._nodes[Data.nodeIndex];

		WorldObject.FaceNode(node, Data.turnSpeed, Data.nodeIndex, Data.autoFocusLantern, true);
	}
}
