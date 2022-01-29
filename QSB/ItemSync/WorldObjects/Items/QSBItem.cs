using Cysharp.Threading.Tasks;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items
{
	internal class QSBItem<T> : WorldObject<T>, IQSBItem
		where T : OWItem
	{
		private QSBItemSocket InitialSocket { get; set; }
		private Transform InitialParent { get; set; }
		private Vector3 InitialPosition { get; set; }
		private Quaternion InitialRotation { get; set; }
		private QSBSector InitialSector { get; set; }

		public override async UniTask Init(CancellationToken cancellationToken)
		{
			if (AttachedObject == null)
			{
				DebugLog.ToConsole($"Error - AttachedObject is null! Type:{GetType().Name}", OWML.Common.MessageType.Error);
				return;
			}

			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => QSBWorldSync.AllObjectsAdded, () =>
			{
				FinishDelayedReady();

				InitialParent = AttachedObject.transform.parent;
				InitialPosition = AttachedObject.transform.localPosition;
				InitialRotation = AttachedObject.transform.localRotation;
				var initialSector = AttachedObject.GetSector();
				if (initialSector != null)
				{
					InitialSector = initialSector.GetWorldObject<QSBSector>();
				}

				if (InitialParent == null)
				{
					DebugLog.ToConsole($"Warning - InitialParent of {AttachedObject.name} is null!", OWML.Common.MessageType.Warning);
				}

				if (InitialParent?.GetComponent<OWItemSocket>() != null)
				{
					var qsbObj = InitialParent.GetComponent<OWItemSocket>().GetWorldObject<QSBItemSocket>();
					InitialSocket = qsbObj;
				}
			});

			QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
		}

		public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

		private void OnPlayerLeave(PlayerInfo player)
		{
			if (player.HeldItem != this)
			{
				return;
			}

			if (InitialSocket != null)
			{
				InitialSocket.PlaceIntoSocket(this);
				return;
			}

			AttachedObject.transform.parent = InitialParent;
			AttachedObject.transform.localPosition = InitialPosition;
			AttachedObject.transform.localRotation = InitialRotation;
			AttachedObject.transform.localScale = Vector3.one;
			AttachedObject.SetSector(InitialSector?.AttachedObject);
			AttachedObject.SetColliderActivation(true);
		}

		public override void SendInitialState(uint to)
		{
			// todo SendInitialState
		}

		public ItemType GetItemType()
			=> AttachedObject.GetItemType();

		public void PickUpItem(Transform holdTransform)
			=> AttachedObject.PickUpItem(holdTransform);

		public void DropItem(Vector3 position, Vector3 normal, Sector sector)
		{
			AttachedObject.transform.SetParent(sector.transform);
			AttachedObject.transform.localScale = Vector3.one;
			var localDropNormal = AttachedObject._localDropNormal;
			var lhs = Quaternion.FromToRotation(AttachedObject.transform.TransformDirection(localDropNormal), normal);
			AttachedObject.transform.rotation = lhs * AttachedObject.transform.rotation;
			var localDropOffset = AttachedObject._localDropOffset;
			AttachedObject.transform.position = sector.transform.TransformPoint(position) + AttachedObject.transform.TransformDirection(localDropOffset);
			AttachedObject.SetSector(sector);
			AttachedObject.SetColliderActivation(true);
		}

		public void OnCompleteUnsocket()
			=> AttachedObject.OnCompleteUnsocket();
	}
}
