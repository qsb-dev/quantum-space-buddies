using QSB.ItemSync.WorldObjects.Items;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets
{
	// todo make this QSBOWItemSocket since it's identical
	internal class QSBOWItemDoubleSocket<T> : WorldObject<T>, IQSBOWItemSocket
		where T : OWItemSocket
	{
		public override void SendResyncInfo(uint to) { }

		public virtual bool AcceptsItem(IQSBOWItem item)
		{
			var itemType = item.GetItemType();
			var acceptableType = AttachedObject._acceptableType;
			return (itemType & acceptableType) == itemType;
		}

		public virtual bool IsSocketOccupied()
			=> AttachedObject.IsSocketOccupied();

		public virtual bool PlaceIntoSocket(IQSBOWItem item)
			=> AttachedObject.PlaceIntoSocket((OWItem)item.ReturnObject());

		public virtual IQSBOWItem RemoveFromSocket()
			=> AttachedObject.RemoveFromSocket().GetWorldObject<IQSBOWItem>();
	}
}
