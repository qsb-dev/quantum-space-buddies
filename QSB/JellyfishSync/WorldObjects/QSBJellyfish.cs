using QSB.AuthoritySync;
using QSB.JellyfishSync.TransformSync;
using QSB.WorldSync;
using QuantumUNET;
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
				QNetworkServer.Spawn(Object.Instantiate(QSBNetworkManager.Instance.JellyfishPrefab));
			}

			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => TransformSync, () =>
			{
				FinishDelayedReady();

				if (QSBCore.IsHost)
				{
					TransformSync.NetIdentity.RegisterAuthQueue();
				}

				// for when you host/connect mid-game
				TransformSync.NetIdentity.FireAuthQueue(!AttachedObject._jellyfishBody.IsSuspended());
			});
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				TransformSync.NetIdentity.UnregisterAuthQueue();
				QNetworkServer.Destroy(TransformSync.gameObject);
			}
		}


		public bool IsRising
		{
			get => AttachedObject._isRising;
			set
			{
				if (AttachedObject._isRising == value)
				{
					return;
				}

				AttachedObject._isRising = value;
				AttachedObject._attractiveFluidVolume.SetVolumeActivation(!value);
			}
		}

		public bool Align
		{
			set => _alignWithTargetBody.enabled = value;
		}
	}
}
