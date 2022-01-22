using Mirror;
using QSB.Anglerfish.Messages;
using QSB.Anglerfish.TransformSync;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Anglerfish.WorldObjects
{
	public class QSBAngler : WorldObject<AnglerfishController>
	{
		public override bool ShouldDisplayDebug() => false;

		public AnglerTransformSync TransformSync;
		public Transform TargetTransform;
		public Vector3 TargetVelocity { get; private set; }

		private Vector3 _lastTargetPosition;

		public override void Init()
		{
			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.singleton.AnglerPrefab).SpawnWithServerAuthority();
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
				this.SendMessage(new AnglerDataMessage(this) { To = to });
			}
		}

		public void UpdateTargetVelocity()
		{
			if (TargetTransform == null)
			{
				return;
			}

			TargetVelocity = TargetTransform.position - _lastTargetPosition;
			_lastTargetPosition = TargetTransform.position;
		}
	}
}
