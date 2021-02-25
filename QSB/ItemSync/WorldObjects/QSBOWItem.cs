using OWML.Utils;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects
{
	internal class QSBOWItem<T> : WorldObject<T>, IQSBOWItem
		where T : OWItem
	{
		public override void Init(T attachedObject, int id) { }

		public ItemType GetItemType()
			=> AttachedObject.GetItemType();

		public void SetColliderActivation(bool active)
			=> AttachedObject.SetColliderActivation(active);

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
		}

		public virtual void SocketItem(Transform socketTransform, Sector sector)
			=> AttachedObject.SocketItem(socketTransform, sector);

		public virtual void PlaySocketAnimation() { }
		public virtual void PlayUnsocketAnimation() { }
	}
}
