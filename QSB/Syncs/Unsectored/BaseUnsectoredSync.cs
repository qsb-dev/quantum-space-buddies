namespace QSB.Syncs.Unsectored
{
	public abstract class BaseUnsectoredSync : SyncBase
	{
		protected sealed override bool AllowNullReferenceTransform => false;
	}
}