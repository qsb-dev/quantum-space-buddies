using Cysharp.Threading.Tasks;
using Mirror;
using QSB.JellyfishSync.Messages;
using QSB.JellyfishSync.TransformSync;
using QSB.Messaging;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.JellyfishSync.WorldObjects;

public class QSBJellyfish : WorldObject<JellyfishController>
{
	public override bool ShouldDisplayDebug() => false;

	public JellyfishTransformSync TransformSync;

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Spawn(Object.Instantiate(QSBNetworkManager.singleton.JellyfishPrefab));
		}

		await UniTask.WaitUntil(() => TransformSync, cancellationToken: ct);
	}

	public override void OnRemoval()
	{
		if (QSBCore.IsHost)
		{
			NetworkServer.Destroy(TransformSync.gameObject);
		}
	}

	public override void SendInitialState(uint to) =>
		this.SendMessage(new JellyfishRisingMessage(AttachedObject._isRising) { To = to });

	public void SetIsRising(bool value)
	{
		if (AttachedObject._isRising == value)
		{
			return;
		}

		AttachedObject._isRising = value;
		AttachedObject._attractiveFluidVolume.SetVolumeActivation(!value);
	}
}