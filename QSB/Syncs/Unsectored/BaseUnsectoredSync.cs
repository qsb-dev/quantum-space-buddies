namespace QSB.Syncs.Unsectored
{
	public abstract class BaseUnsectoredSync : SyncBase
	{
		protected override bool AllowDisabledAttachedObject => false;
		protected override bool AllowNullReferenceTransform => false;
		protected override bool DestroyAttachedObject => false;
	}
}
