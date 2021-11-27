namespace QSB.ItemSync.WorldObjects.Items
{
	internal class QSBWarpCoreItem : QSBOWItem<WarpCoreItem>
	{
		public override void Init(WarpCoreItem attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}

		public bool IsVesselCoreType()
			=> AttachedObject.IsVesselCoreType();
	}
}
