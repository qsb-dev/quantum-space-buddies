using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Unsectored
{
	public abstract class BaseUnsectoredSync<T> : SyncBase<T> where T : Component
	{
		public override bool IgnoreDisabledAttachedObject => false;
		public override bool IgnoreNullReferenceTransform => false;
		public override bool ShouldReparentAttachedObject => false;

		public override void SerializeTransform(QNetworkWriter writer, bool initialState) { }
	}
}
