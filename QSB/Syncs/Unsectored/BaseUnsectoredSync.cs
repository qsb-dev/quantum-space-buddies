using QuantumUNET.Transport;

namespace QSB.Syncs.Unsectored
{
	public abstract class BaseUnsectoredSync : SyncBase
	{
		public override bool IgnoreDisabledAttachedObject => false;
		public override bool IgnoreNullReferenceTransform => false;
		public override bool ShouldReparentAttachedObject => false;

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}
		}
	}
}
