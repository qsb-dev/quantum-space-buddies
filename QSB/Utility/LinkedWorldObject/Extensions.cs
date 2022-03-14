using Mirror;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Utility.LinkedWorldObject;

public static class Extensions
{
	public static void SpawnLinked(this GameObject go, ILinkedWorldObject<INetworkBehaviour> worldObject)
	{
		var networkBehaviour = go.GetComponent<ILinkedNetworkBehaviour<IWorldObject>>();
		worldObject.LinkTo(networkBehaviour);
		networkBehaviour.LinkTo(worldObject);
		NetworkServer.Spawn(go);
	}
}
