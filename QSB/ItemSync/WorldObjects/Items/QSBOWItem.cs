﻿using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items
{
	internal class QSBOWItem<T> : WorldObject<T>, IQSBOWItem
		where T : OWItem
	{
		public IQSBOWItemSocket InitialSocket { get; private set; }
		public Transform InitialParent { get; private set; }
		public Vector3 InitialPosition { get; private set; }
		public Quaternion InitialRotation { get; private set; }
		public QSBSector InitialSector { get; private set; }
		public uint HoldingPlayer { get; private set; }

		public override void Init(T attachedObject, int id)
		{
			if (attachedObject == null)
			{
				DebugLog.ToConsole($"Error - AttachedObject is null! Type:{GetType().Name}", OWML.Common.MessageType.Error);
				return;
			}

			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllObjectsAdded, () =>
			{
				FinishDelayedReady();

				InitialParent = attachedObject.transform.parent;
				InitialPosition = attachedObject.transform.localPosition;
				InitialRotation = attachedObject.transform.localRotation;
				var initialSector = attachedObject.GetSector();
				if (initialSector != null)
				{
					InitialSector = initialSector.GetWorldObject<QSBSector>();
				}

				if (InitialParent == null)
				{
					DebugLog.ToConsole($"Warning - InitialParent of {attachedObject.name} is null!", OWML.Common.MessageType.Warning);
				}

				if (InitialParent?.GetComponent<OWItemSocket>() != null)
				{
					var qsbObj = (IQSBOWItemSocket)InitialParent.GetComponent<OWItemSocket>().GetWorldObject();
					InitialSocket = qsbObj;
				}
			});

			QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
		}

		public override void OnRemoval() => QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;

		private void OnPlayerLeave(uint player)
		{
			if (HoldingPlayer != player)
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

		public ItemType GetItemType()
			=> AttachedObject.GetItemType();

		public void SetColliderActivation(bool active)
			=> AttachedObject.SetColliderActivation(active);

		public virtual void SocketItem(Transform socketTransform, Sector sector)
		{
			AttachedObject.SocketItem(socketTransform, sector);
			HoldingPlayer = 0;
		}

		public virtual void PickUpItem(Transform holdTransform, uint playerId)
		{
			AttachedObject.PickUpItem(holdTransform);
			HoldingPlayer = playerId;
		}

		public virtual void DropItem(Vector3 position, Vector3 normal, Sector sector)
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
			HoldingPlayer = 0;
		}

		public virtual void PlaySocketAnimation() { }
		public virtual void PlayUnsocketAnimation() { }
		public virtual void OnCompleteUnsocket() { }
	}
}
