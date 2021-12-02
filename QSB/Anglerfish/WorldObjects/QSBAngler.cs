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
				QNetworkServer.Spawn(Object.Instantiate(QSBNetworkManager.Instance.AnglerPrefab));
				QSBCore.UnityEvents.RunWhen(() => TransformSync, () =>
					TransformSync.NetIdentity.RegisterAuthQueue());
			}

			// for when you host/connect mid-game
			QSBCore.UnityEvents.RunWhen(() => TransformSync, () =>
				TransformSync.NetIdentity.FireAuthQueue(!AttachedObject._anglerBody.IsSuspended()));
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
