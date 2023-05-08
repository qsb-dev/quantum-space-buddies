using Cysharp.Threading.Tasks;
using Mirror;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.Utility.LinkedWorldObject;

/// <summary>
/// helper implementation of the interface
/// </summary>
public abstract class LinkedWorldObject<T, TNetworkBehaviour> : WorldObject<T>, ILinkedWorldObject<TNetworkBehaviour>
	where T : MonoBehaviour
	where TNetworkBehaviour : NetworkBehaviour
{
	public TNetworkBehaviour NetworkBehaviour { get; private set; }
	public void SetNetworkBehaviour(NetworkBehaviour networkBehaviour) => NetworkBehaviour = (TNetworkBehaviour)networkBehaviour;

	protected abstract GameObject NetworkObjectPrefab { get; }
	protected abstract bool SpawnWithServerOwnership { get; }

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			this.SpawnLinked(NetworkObjectPrefab, SpawnWithServerOwnership);
		}
		else
		{
			await this.WaitForLink(ct);
		}
	}

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(NetworkBehaviour.gameObject);
		}
	}
}
