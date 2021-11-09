namespace QSB.ItemSync.WorldObjects.Sockets
{
	internal class QSBSlideProjectorSocket : QSBOWItemDoubleSocket<SlideProjectorSocket>
	{
		public override void Init(SlideProjectorSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
