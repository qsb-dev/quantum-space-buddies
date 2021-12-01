using QSB.Events;
using QSB.JellyfishSync.TransformSync;
using QSB.SuspendableSync;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.JellyfishSync.WorldObjects
{
	public class QSBJellyfish : WorldObject<JellyfishController>
	{
		public JellyfishTransformSync TransformSync;

		public override void Init(JellyfishController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;

			if (QSBCore.IsHost)
			{
				QNetworkServer.Spawn(Object.Instantiate(QSBNetworkManager.Instance.JellyfishPrefab));
				QSBCore.UnityEvents.RunWhen(() => TransformSync, () =>
					SuspendableManager.Register(TransformSync.NetIdentity));
			}

			// for when you host/connect mid-game
			QSBCore.UnityEvents.RunWhen(() => TransformSync, () =>
				QSBEventManager.FireEvent(EventNames.QSBSuspendChange, TransformSync.NetIdentity, AttachedObject._jellyfishBody.IsSuspended()));
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				SuspendableManager.Unregister(TransformSync.NetIdentity);
				QNetworkServer.Destroy(TransformSync.gameObject);
			}
		}
	}
}
