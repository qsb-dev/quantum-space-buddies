using QSB.JellyfishSync.Messages;
using QSB.JellyfishSync.TransformSync;
using QSB.Messaging;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.JellyfishSync.WorldObjects;

public class QSBJellyfish : LinkedWorldObject<JellyfishController, JellyfishTransformSync>
{
	public override bool ShouldDisplayDebug() => false;

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.JellyfishPrefab;
	protected override bool SpawnWithServerOwnership => false;

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
