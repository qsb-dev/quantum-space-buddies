using Mirror;
using QSB.JellyfishSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.JellyfishSync.WorldObjects
{
	public class QSBJellyfish : WorldObject<JellyfishController>
	{
		public JellyfishTransformSync TransformSync;
		private AlignWithTargetBody _alignWithTargetBody;

		public override void Init()
		{
			_alignWithTargetBody = AttachedObject.GetRequiredComponent<AlignWithTargetBody>();

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
