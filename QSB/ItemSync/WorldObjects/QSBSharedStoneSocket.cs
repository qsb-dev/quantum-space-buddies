namespace QSB.ItemSync.WorldObjects
{
	internal class QSBSharedStoneSocket : QSBOWItemSocket<SharedStoneSocket>
	{
		public override void Init(SharedStoneSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
