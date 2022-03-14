using Cysharp.Threading.Tasks;
using Mirror;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Utility.LinkedWorldObject;

public static class Extensions
{
	/// <summary>
	/// link a world object and network object, then spawn it.
	/// (host only)
	/// </summary>
	public static void SpawnLinked(this ILinkedWorldObject<NetworkBehaviour> worldObject, GameObject prefab)
	{
		var networkBehaviour = prefab.GetComponent<ILinkedNetworkBehaviour<IWorldObject>>();

		worldObject.LinkTo((NetworkBehaviour)networkBehaviour);
		networkBehaviour.LinkTo(worldObject);

		NetworkServer.Spawn(prefab);
	}

	/// <summary>
	/// wait for a world object to be linked.
	/// (non host only)
	/// </summary>
	public static async UniTask WaitForLink(this ILinkedWorldObject<NetworkBehaviour> worldObject) =>
		UniTask.WaitUntil(() => worldObject.NetworkBehaviour);
}
