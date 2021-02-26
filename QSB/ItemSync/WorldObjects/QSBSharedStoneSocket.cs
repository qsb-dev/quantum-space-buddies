namespace QSB.ItemSync.WorldObjects
{
	class QSBSharedStoneSocket : QSBOWItemSocket<SharedStoneSocket>
	{
		public override void Init(SharedStoneSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
