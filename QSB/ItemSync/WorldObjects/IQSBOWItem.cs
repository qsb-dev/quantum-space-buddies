using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects
{
	public interface IQSBOWItem : IWorldObjectTypeSubset
	{
		ItemType GetItemType();
		void SetColliderActivation(bool active);
		void SocketItem(Transform socketTransform, Sector sector);
		void PickUpItem(Transform holdTransform, uint playerId);
		void DropItem(Vector3 position, Vector3 normal, Sector sector);
		void PlaySocketAnimation();
		void PlayUnsocketAnimation();
		void OnCompleteUnsocket();
	}
}
