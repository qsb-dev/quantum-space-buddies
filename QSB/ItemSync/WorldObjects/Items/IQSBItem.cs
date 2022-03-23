using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items;

public interface IQSBItem : IWorldObject
{
	ItemType GetItemType();
	void PickUpItem(Transform itemSocket);
	void DropItem(Vector3 position, Vector3 normal, Sector sector);
	void OnCompleteUnsocket();

	/// <summary>
	/// store the last location when a remote player drops/sockets us
	/// so we can use it if they leave while still holding
	/// </summary>
	void StoreLocation();
}