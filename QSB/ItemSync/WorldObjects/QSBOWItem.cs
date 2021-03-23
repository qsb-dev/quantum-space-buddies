using OWML.Utils;
using QSB.Player;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects
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
			InitialParent = attachedObject.transform.parent;
			InitialPosition = attachedObject.transform.localPosition;
			InitialRotation = attachedObject.transform.localRotation;
			InitialSector = QSBWorldSync.GetWorldFromUnity<QSBSector, Sector>(attachedObject.GetSector());
			if (InitialParent.GetComponent<OWItemSocket>() != null)
			{
				var qsbObj = ItemManager.GetObject(InitialParent.GetComponent<OWItemSocket>());
				InitialSocket = qsbObj;
			}
			QSBPlayerManager.OnRemovePlayer += OnPlayerLeave;
		}

		public override void OnRemoval()
		{
			QSBPlayerManager.OnRemovePlayer -= OnPlayerLeave;
		}

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
			DebugLog.DebugWrite($"OnPlayerLeave {player} for item {AttachedObject.name}");
			AttachedObject.transform.parent = InitialParent;
			AttachedObject.transform.localPosition = InitialPosition;
			AttachedObject.transform.localRotation = InitialRotation;
			AttachedObject.transform.localScale = Vector3.one;
			AttachedObject.SetSector(InitialSector.AttachedObject);
			AttachedObject.SetColliderActivation(true);
		}

		public ItemType GetItemType()
			=> AttachedObject.GetItemType();

		public void SetColliderActivation(bool active)
			=> AttachedObject.SetColliderActivation(active);

		public virtual void SocketItem(Transform socketTransform, Sector sector)
		{
			AttachedObject.SocketItem(socketTransform, sector);
			DebugLog.DebugWrite($"{AttachedObject.name} set holding to 0");
			HoldingPlayer = 0;
		}


		public virtual void PickUpItem(Transform holdTransform, uint playerId)
		{
			AttachedObject.PickUpItem(holdTransform);
			DebugLog.DebugWrite($"{AttachedObject.name} set holding to {playerId}");
			HoldingPlayer = playerId;
		}

		public virtual void DropItem(Vector3 position, Vector3 normal, Sector sector)
		{
			AttachedObject.transform.SetParent(sector.transform);
			AttachedObject.transform.localScale = Vector3.one;
			var localDropNormal = AttachedObject.GetValue<Vector3>("_localDropNormal");
			var lhs = Quaternion.FromToRotation(AttachedObject.transform.TransformDirection(localDropNormal), normal);
			AttachedObject.transform.rotation = lhs * AttachedObject.transform.rotation;
			var localDropOffset = AttachedObject.GetValue<Vector3>("_localDropOffset");
			AttachedObject.transform.position = sector.transform.TransformPoint(position) + AttachedObject.transform.TransformDirection(localDropOffset);
			AttachedObject.SetSector(sector);
			AttachedObject.SetColliderActivation(true);
			DebugLog.DebugWrite($"{AttachedObject.name} set holding to 0");
			HoldingPlayer = 0;
		}

		public virtual void PlaySocketAnimation() { }
		public virtual void PlayUnsocketAnimation() { }
		public virtual void OnCompleteUnsocket() { }
	}
}
