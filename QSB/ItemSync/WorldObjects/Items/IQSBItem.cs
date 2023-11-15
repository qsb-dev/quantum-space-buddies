using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items;

public interface IQSBItem : IWorldObject
{
	ItemType GetItemType();
	void PickUpItem(Transform itemSocket);
	void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget);
	void OnCompleteUnsocket();

	/// <summary>
	/// store the last location when a remote player picks up/unsockets an item
	/// so we can drop/socket it if they leave while still holding it
	/// </summary>
	void StoreLocation();
}