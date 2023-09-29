namespace QSB.ItemSync.WorldObjects.Items;

public class QSBWarpCoreItem : QSBItem<WarpCoreItem>
{
	public bool IsVesselCoreType() => AttachedObject.IsVesselCoreType();
}