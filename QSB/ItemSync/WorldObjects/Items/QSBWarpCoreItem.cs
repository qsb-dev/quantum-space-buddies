namespace QSB.ItemSync.WorldObjects.Items
{
	internal class QSBWarpCoreItem : QSBItem<WarpCoreItem>
	{
		public bool IsVesselCoreType() => AttachedObject.IsVesselCoreType();
	}
}