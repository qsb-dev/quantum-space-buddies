using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items
{
	public interface IQSBItem : IWorldObject
	{
		ItemType GetItemType();
		void PickUpItem(Transform itemSocket);
		void DropItem(Vector3 position, Vector3 normal, Sector sector);
		void OnCompleteUnsocket();
	}
}