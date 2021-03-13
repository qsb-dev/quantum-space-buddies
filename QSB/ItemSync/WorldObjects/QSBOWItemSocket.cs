using OWML.Utils;
using QSB.WorldSync;

namespace QSB.ItemSync.WorldObjects
{
	internal class QSBOWItemSocket<T> : WorldObject<T>, IQSBOWItemSocket
		where T : OWItemSocket
	{
		public override void Init(T attachedObject, int id) { }

		public virtual bool AcceptsItem(IQSBOWItem item)
		{
			var itemType = item.GetItemType();
			var acceptableType = AttachedObject.GetValue<ItemType>("_acceptableType");
			return (itemType & acceptableType) == itemType;
		}

		public virtual bool PlaceIntoSocket(IQSBOWItem item)
			=> AttachedObject.PlaceIntoSocket((OWItem)(item as IWorldObject).ReturnObject());

		public virtual IQSBOWItem RemoveFromSocket()
			=> ItemManager.GetObject(AttachedObject.RemoveFromSocket());
	}
}
