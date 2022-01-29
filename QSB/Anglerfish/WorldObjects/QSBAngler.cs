using Cysharp.Threading.Tasks;
using Mirror;
using QSB.Anglerfish.Messages;
using QSB.Anglerfish.TransformSync;
using QSB.Messaging;
using QSB.WorldSync;
using System.Threading;
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

		public override async UniTask Init(CancellationToken ct)
		{
			if (QSBCore.IsHost)
			{
				NetworkServer.Spawn(Object.Instantiate(QSBNetworkManager.singleton.AnglerPrefab));
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

		public override void SendInitialState(uint to)
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
