using Cysharp.Threading.Tasks;
using Mirror;
using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.RaftSync.WorldObjects;

public class QSBRaft : WorldObject<RaftController>
{
	public override bool ShouldDisplayDebug() => false;

	public RaftTransformSync TransformSync;

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			Object.Instantiate(QSBNetworkManager.singleton.RaftPrefab).SpawnWithServerAuthority();
		}

		await UniTask.WaitUntil(() => TransformSync, cancellationToken: ct);

		AttachedObject._interactReceiver.OnPressInteract += OnPressInteract;
	}

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(TransformSync.gameObject);
		}

		AttachedObject._interactReceiver.OnPressInteract -= OnPressInteract;
	}

	public override void SendInitialState(uint to)
	{
		// todo?? SendInitialState
	}

	private void OnPressInteract() =>
		TransformSync.netIdentity.UpdateAuthQueue(AuthQueueAction.Force);
}
