namespace QSB.ItemSync.WorldObjects
{
	internal class QSBScrollSocket : QSBOWItemSocket<ScrollSocket>
	{
		public override void Init(ScrollSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}

		public override IQSBOWItem RemoveFromSocket()
		{
			var scrollItem = (QSBScrollItem)_socketedItem;
			scrollItem.HideNomaiText();
			return base.RemoveFromSocket();
		}
	}
}
