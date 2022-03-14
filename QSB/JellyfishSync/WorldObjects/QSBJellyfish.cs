using Cysharp.Threading.Tasks;
using Mirror;
using QSB.JellyfishSync.Messages;
using QSB.JellyfishSync.TransformSync;
using QSB.Messaging;
using QSB.Utility.LinkedWorldObject;
using QSB.WorldSync;
using System.Threading;

namespace QSB.JellyfishSync.WorldObjects;

public class QSBJellyfish : WorldObject<JellyfishController>, ILinkedWorldObject<JellyfishTransformSync>
{
	public override bool ShouldDisplayDebug() => false;

	public JellyfishTransformSync NetworkBehaviour { get; private set; }
	public void LinkTo(NetworkBehaviour networkBehaviour) => NetworkBehaviour = (JellyfishTransformSync)networkBehaviour;

	public override async UniTask Init(CancellationToken ct)
	{
		if (QSBCore.IsHost)
		{
			this.SpawnLinked(QSBNetworkManager.singleton.JellyfishPrefab);
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
