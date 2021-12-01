using QSB.Anglerfish.TransformSync;
using QSB.Events;
using QSB.SuspendableSync;
using QSB.Utility;
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
				QSBCore.UnityEvents.RunWhen(() => TransformSync, () =>
					SuspendableManager.Register(TransformSync.NetIdentity));
				Object.Instantiate(QSBNetworkManager.Instance.AnglerPrefab).SpawnWithServerAuthority();
			}

			// for when you host/connect mid-game
			QSBCore.UnityEvents.RunWhen(() => TransformSync, () =>
				QSBEventManager.FireEvent(EventNames.QSBSuspendChange, TransformSync.NetIdentity, AttachedObject._anglerBody.IsSuspended()));
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				SuspendableManager.Unregister(TransformSync.NetIdentity);
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
