using Mirror;

namespace QSB.Syncs.Unsectored
{
	public abstract class BaseUnsectoredSync : SyncBase
	{
		public override bool IgnoreDisabledAttachedObject => false;
		public override bool IgnoreNullReferenceTransform => false;
		public override bool DestroyAttachedObject => false;

		protected override void Serialize(NetworkWriter writer, bool initialState) { }
	}
}
