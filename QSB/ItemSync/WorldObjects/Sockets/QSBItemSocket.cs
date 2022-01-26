using QSB.ItemSync.WorldObjects.Items;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets
{
	public class QSBItemSocket : WorldObject<OWItemSocket>
	{
		public override void SendInitialState(uint to)
		{
			// todo SendResyncInfo
		}

		public bool IsSocketOccupied()
			=> AttachedObject.IsSocketOccupied();

		public void PlaceIntoSocket(QSBItem item)
			=> AttachedObject.PlaceIntoSocket(item.AttachedObject);

		public void RemoveFromSocket()
			=> AttachedObject.RemoveFromSocket();
	}
}
