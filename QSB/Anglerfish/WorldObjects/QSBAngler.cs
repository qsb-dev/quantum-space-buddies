using QSB.Anglerfish.TransformSync;
using QSB.Events;
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

			QSBCore.UnityEvents.RunWhen(() => TransformSync, () =>
			{
				AttachedObject.OnAnglerSuspended += OnSuspend;
				AttachedObject.OnAnglerUnsuspended += OnUnsuspend;
			});

			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.Instance.AnglerPrefab).SpawnWithServerAuthority();
			}
		}

		public override void OnRemoval()
		{
			AttachedObject.OnAnglerSuspended -= OnSuspend;
			AttachedObject.OnAnglerUnsuspended -= OnUnsuspend;

			if (QSBCore.IsHost)
			{
				QNetworkServer.Destroy(TransformSync.gameObject);
			}
		}

		private void OnSuspend(AnglerfishController.AnglerState _) =>
			QSBEventManager.FireEvent(EventNames.QSBSuspensionChange, TransformSync.NetIdentity, true);

		private void OnUnsuspend(AnglerfishController.AnglerState _) =>
			QSBEventManager.FireEvent(EventNames.QSBSuspensionChange, TransformSync.NetIdentity, false);

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
