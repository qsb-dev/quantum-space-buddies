namespace QSB.ItemSync.WorldObjects.Sockets
{
	internal class QSBWarpCoreSocket : QSBOWItemSocket<WarpCoreSocket>
	{
		public override void Init(WarpCoreSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
