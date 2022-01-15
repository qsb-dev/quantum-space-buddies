using Mirror;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Unsectored
{
	public abstract class BaseUnsectoredSync2 : SyncBase2
	{
		public override bool IgnoreDisabledAttachedObject => false;
		public override bool IgnoreNullReferenceTransform => false;
		public override bool DestroyAttachedObject => false;

		protected override void Serialize(NetworkWriter writer, bool initialState) { }
	}

	public abstract class BaseUnsectoredSync<T> : SyncBase<T> where T : Component
	{
		public override bool IgnoreDisabledAttachedObject => false;
		public override bool IgnoreNullReferenceTransform => false;
		public override bool DestroyAttachedObject => false;

		public override void SerializeTransform(QNetworkWriter writer, bool initialState) { }
	}
}
