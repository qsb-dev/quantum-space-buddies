using QSB.ItemSync.WorldObjects.Items;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects.Sockets
{
	internal class QSBOWItemSocket<T> : WorldObject<T>, IQSBOWItemSocket
		where T : OWItemSocket
	{
		public virtual bool AcceptsItem(IQSBOWItem item)
		{
			var itemType = item.GetItemType();
			var acceptableType = AttachedObject._acceptableType;
			return (itemType & acceptableType) == itemType;
		}

		public virtual bool IsSocketOccupied()
			=> AttachedObject.IsSocketOccupied();

		public virtual bool PlaceIntoSocket(IQSBOWItem item)
			=> AttachedObject.PlaceIntoSocket((OWItem)(item as IWorldObject).ReturnObject());

		public virtual IQSBOWItem RemoveFromSocket()
			=> (IQSBOWItem)QSBWorldSync.GetWorldFromUnity(AttachedObject.RemoveFromSocket());
	}
}
