namespace QSB.ItemSync.WorldObjects.Items
{
	internal class QSBWarpCoreItem : QSBOWItem<WarpCoreItem>
	{
		public bool IsVesselCoreType()
			=> AttachedObject.IsVesselCoreType();
	}
}
