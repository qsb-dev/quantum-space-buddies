using Cysharp.Threading.Tasks;
using Mirror;
using System.Threading;
using UnityEngine;

namespace QSB.Utility.LinkedWorldObject;

public static class ILinkedWorldObject_Extensions
{
	/// <summary>
	/// link a world object and a network behaviour
	/// </summary>
	public static void LinkTo(this ILinkedWorldObject<NetworkBehaviour> worldObject, ILinkedNetworkBehaviour networkBehaviour)
	{
		worldObject.SetNetworkBehaviour((NetworkBehaviour)networkBehaviour);
		networkBehaviour.SetWorldObject(worldObject);
	}

	/// <summary>
	/// link a world object and network object, then spawn it.
	/// (host only)
	/// </summary>
	public static void SpawnLinked(this ILinkedWorldObject<NetworkBehaviour> worldObject, GameObject prefab, bool spawnWithServerAuthority)
	{
		var go = Object.Instantiate(prefab);
		var networkBehaviour = go.GetComponent<ILinkedNetworkBehaviour>();

		worldObject.LinkTo(networkBehaviour);

		if (spawnWithServerAuthority)
		{
			go.SpawnWithServerAuthority();
		}
		else
		{
			NetworkServer.Spawn(go);
		}
	}

	/// <summary>
	/// wait for a world object to be linked.
	/// (non host only)
	/// </summary>
	public static async UniTask WaitForLink(this ILinkedWorldObject<NetworkBehaviour> worldObject, CancellationToken ct) =>
		await UniTask.WaitUntil(() => worldObject.NetworkBehaviour, cancellationToken: ct);
}
