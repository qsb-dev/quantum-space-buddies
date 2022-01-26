using QSB.ItemSync.WorldObjects.Items;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets
{
	internal class QSBItemSocket : WorldObject<OWItemSocket>
	{
		public override void SendResyncInfo(uint to)
		{
			// todo SendResyncInfo
		}

		public bool AcceptsItem(IQSBOWItem item)
		{
			var itemType = item.GetItemType();
			var acceptableType = AttachedObject._acceptableType;
			return (itemType & acceptableType) == itemType;
		}

		public bool IsSocketOccupied()
			=> AttachedObject.IsSocketOccupied();

		public bool PlaceIntoSocket(IQSBOWItem item)
			=> AttachedObject.PlaceIntoSocket((OWItem)item.ReturnObject());

		public IQSBOWItem RemoveFromSocket()
			=> AttachedObject.RemoveFromSocket().GetWorldObject<IQSBOWItem>();
	}
}
