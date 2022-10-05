using QSB.ItemSync.WorldObjects.Items;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets;

internal class QSBItemSocket : WorldObject<OWItemSocket>
{
	public bool IsSocketOccupied() => AttachedObject.IsSocketOccupied();

	public void PlaceIntoSocket(IQSBItem item)
		=> AttachedObject.PlaceIntoSocket((OWItem)item.AttachedObject);

	public void RemoveFromSocket()
		=> AttachedObject.RemoveFromSocket();
}
