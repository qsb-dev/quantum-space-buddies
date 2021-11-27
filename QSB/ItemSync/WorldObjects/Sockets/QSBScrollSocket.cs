namespace QSB.ItemSync.WorldObjects.Sockets
{
	internal class QSBScrollSocket : QSBOWItemSocket<ScrollSocket>
	{
		public override void Init(ScrollSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
