using Cysharp.Threading.Tasks;
using Mirror;
using QSB.WorldSync;
using System.Linq;
using System.Threading;
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
		var go = Object.Instantiate(prefab);
		var networkIdentity = go.GetComponent<NetworkIdentity>();
		var networkBehaviour = networkIdentity.NetworkBehaviours.OfType<ILinkedNetworkBehaviour<IWorldObject>>().First();

		worldObject.LinkTo((NetworkBehaviour)networkBehaviour);
		networkBehaviour.LinkTo(worldObject);

		NetworkServer.Spawn(go);
	}

	/// <summary>
	/// wait for a world object to be linked.
	/// (non host only)
	/// </summary>
	public static async UniTask WaitForLink(this ILinkedWorldObject<NetworkBehaviour> worldObject, CancellationToken ct) =>
		await UniTask.WaitUntil(() => worldObject.NetworkBehaviour, cancellationToken: ct);
}
