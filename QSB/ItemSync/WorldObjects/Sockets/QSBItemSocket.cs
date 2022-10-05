using QSB.ItemSync.WorldObjects.Items;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets;

internal class QSBItemSocket : WorldObject<OWItemSocket>
{
	public bool IsSocketOccupied() => AttachedObject.IsSocketOccupied();

	public void PlaceIntoSocket(IQSBItem item)
	{
		AttachedObject.PlaceIntoSocket((OWItem)item.AttachedObject);

		// Don't let other users unsocket a DreamLantern in the dreamworld that doesn't belong to them
		// DreamLanternSockets only exist in the DreamWorld
		AttachedObject.EnableInteraction(AttachedObject is not DreamLanternSocket);
	}

	public void RemoveFromSocket()
	{
		AttachedObject.RemoveFromSocket();

		AttachedObject.EnableInteraction(true);
	}
}
