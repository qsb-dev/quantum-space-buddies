using Mirror;
using QSB.JellyfishSync.Messages;
using QSB.JellyfishSync.TransformSync;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.JellyfishSync.WorldObjects
{
	public class QSBJellyfish : WorldObject<JellyfishController>
	{
		public JellyfishTransformSync TransformSync;

		public override void Init()
		{
			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.singleton.JellyfishPrefab).SpawnWithServerAuthority();
			}

			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => TransformSync, FinishDelayedReady);
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				NetworkServer.Destroy(TransformSync.gameObject);
			}
		}

		public override void SendResyncInfo(uint to)
		{
			if (TransformSync.hasAuthority)
			{
				this.SendMessage(new JellyfishRisingMessage(AttachedObject._isRising) { To = to });
			}
		}

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
}
