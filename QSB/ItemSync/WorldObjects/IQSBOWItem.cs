using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects
{
	public interface IQSBOWItem : IWorldObjectTypeSubset
	{
		void PickUpItem(Transform holdTransform);
		void DropItem(Vector3 position, Vector3 normal, Sector sector);
		ItemType GetItemType();
		void SocketItem(Transform socketTransform, Sector sector);
		void SetColliderActivation(bool active);
		void PlaySocketAnimation();
		void PlayUnsocketAnimation();
	}
}
