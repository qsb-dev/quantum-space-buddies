using Cysharp.Threading.Tasks;
using QSB.JellyfishSync.Messages;
using QSB.JellyfishSync.TransformSync;
using QSB.Messaging;
using QSB.Utility;
using QSB.Utility.LinkedWorldObject;
using System.Threading;
using UnityEngine;

namespace QSB.JellyfishSync.WorldObjects;

public class QSBJellyfish : LinkedWorldObject<JellyfishController, JellyfishTransformSync>
{
	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		if (QSBCore.IsHost)
		{
			AttachedObject._upwardsAcceleration *= 10;
			AttachedObject._downwardsAcceleration *= 10;
		}
	}

	public override bool ShouldDisplayDebug() => false;

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.JellyfishPrefab;
	protected override bool SpawnWithServerAuthority => false;

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
		DebugLog.DebugWrite($"{this} SET IS RISING");
	}
}
