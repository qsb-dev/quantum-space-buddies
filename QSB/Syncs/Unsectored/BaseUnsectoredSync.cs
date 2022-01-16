namespace QSB.Syncs.Unsectored
{
	public abstract class BaseUnsectoredSync : SyncBase
	{
		public override bool AllowDisabledAttachedObject => false;
		public override bool AllowNullReferenceTransform => false;
		public override bool DestroyAttachedObject => false;
	}
}
