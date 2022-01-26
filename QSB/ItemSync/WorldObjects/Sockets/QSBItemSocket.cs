using QSB.ItemSync.WorldObjects.Items;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets
{
	public class QSBItemSocket : WorldObject<OWItemSocket>
	{
		public override void SendResyncInfo(uint to)
		{
			// todo SendResyncInfo
		}

		public bool AcceptsItem(QSBItem item)
		{
			var itemType = item.GetItemType();
			var acceptableType = AttachedObject._acceptableType;
			return (itemType & acceptableType) == itemType;
		}

		public bool IsSocketOccupied()
			=> AttachedObject.IsSocketOccupied();

		public bool PlaceIntoSocket(QSBItem item)
			=> AttachedObject.PlaceIntoSocket(item.AttachedObject);

		public QSBItem RemoveFromSocket()
			=> AttachedObject.RemoveFromSocket().GetWorldObject<QSBItem>();
	}
}
