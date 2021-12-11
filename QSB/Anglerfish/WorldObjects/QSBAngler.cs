using QSB.Anglerfish.TransformSync;
using QSB.AuthoritySync;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.Anglerfish.WorldObjects
{
	public class QSBAngler : WorldObject<AnglerfishController>
	{
		public AnglerTransformSync TransformSync;
		public Transform TargetTransform;
		public Vector3 TargetVelocity { get; private set; }

		private Vector3 _lastTargetPosition;

		public override void Init(AnglerfishController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;

			if (QSBCore.IsHost)
			{
				var transformSync = Object.Instantiate(QSBNetworkManager.Instance.AnglerPrefab).GetComponent<AnglerTransformSync>();
				transformSync.ObjectId = ObjectId;
				QNetworkServer.Spawn(transformSync.gameObject);
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
				TransformSync.NetIdentity.FireAuthQueue(!AttachedObject._anglerBody.IsSuspended());
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
