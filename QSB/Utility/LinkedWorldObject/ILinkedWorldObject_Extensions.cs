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
	public static void LinkTo(this ILinkedWorldObject<NetworkBehaviour> @this, ILinkedNetworkBehaviour networkBehaviour)
	{
		@this.SetNetworkBehaviour((NetworkBehaviour)networkBehaviour);
		networkBehaviour.SetWorldObject(@this);
	}

	/// <summary>
	/// link a world object and network object, then spawn it.
	/// (host only)
	/// </summary>
	public static void SpawnLinked(this ILinkedWorldObject<NetworkBehaviour> @this, GameObject prefab, bool spawnWithServerOwnership)
	{
		var go = Object.Instantiate(prefab);
		var networkBehaviour = go.GetComponent<ILinkedNetworkBehaviour>();

		@this.LinkTo(networkBehaviour);

		if (spawnWithServerOwnership)
		{
			go.SpawnWithServerOwnership();
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
	public static async UniTask WaitForLink(this ILinkedWorldObject<NetworkBehaviour> @this, CancellationToken ct) =>
		await UniTask.WaitUntil(() => @this.NetworkBehaviour, cancellationToken: ct);
}
