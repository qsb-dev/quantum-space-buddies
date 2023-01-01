using GhostEnums;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class FaceNodeListMessage : QSBWorldObjectMessage<QSBGhostController, (int mapId, int[] nodeIndexes, int numNodes, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern)>
{
	public FaceNodeListMessage(GhostNode[] nodeList, int numNodes, TurnSpeed turnSpeed, float nodeDelay, bool autoFocus) : base(Process(nodeList, numNodes, turnSpeed, nodeDelay, autoFocus)) { }

	private static (int mapId, int[] nodeIndexes, int numNodes, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern) Process(GhostNode[] nodeList, int numNodes, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern)
	{
		(int mapId, int[] nodeIndexes, int numNodes, TurnSpeed turnSpeed, float nodeDelay, bool autoFocusLantern) ret = new()
		{
			numNodes = numNodes,
			turnSpeed = turnSpeed,
			nodeDelay = nodeDelay,
			autoFocusLantern = autoFocusLantern
		};

		if (numNodes == 0)
		{
			ret.mapId = -1;
			ret.nodeIndexes = new int[numNodes];
			return ret;
		}

		var nodeMaps = QSBWorldSync.GetWorldObjects<QSBGhostNodeMap>();
		var owner = nodeMaps.First(x => x.AttachedObject._nodes.Contains(nodeList[0]));

		var hasAll = nodeList.All(owner.AttachedObject._nodes.Contains);

		if (!hasAll)
		{
			DebugLog.ToConsole($"Warning - owner.nodes does not contain all of nodelist! Trying to find a correct owner...", OWML.Common.MessageType.Warning);

			owner = nodeMaps.FirstOrDefault(x => nodeList.All(x.AttachedObject._nodes.Contains));

			if (owner == default)
			{
				DebugLog.ToConsole($"Error - Failed to find correct owner for nodelist.", OWML.Common.MessageType.Error);
				ret.mapId = -1;
				ret.nodeIndexes = new int[numNodes];
			}
		}

		ret.mapId = owner.ObjectId;
		ret.nodeIndexes = nodeList.Select(x => Array.IndexOf(owner.AttachedObject._nodes, x)).ToArray();

		return ret;
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			DebugLog.ToConsole("Error - Received FaceNodeListMessage on host. Something has gone horribly wrong!", OWML.Common.MessageType.Error);
			return;
		}

		var map = Data.mapId.GetWorldObject<QSBGhostNodeMap>();
		var nodeList = Data.nodeIndexes.Select(x => map.AttachedObject._nodes[x]).ToArray();

		WorldObject.FaceNodeList(nodeList, Data.numNodes, Data.turnSpeed, Data.nodeDelay, Data.autoFocusLantern, true);
	}
}
