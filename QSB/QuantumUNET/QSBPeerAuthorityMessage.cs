using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBPeerAuthorityMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader)
		{
			this.connectionId = (int)reader.ReadPackedUInt32();
			this.netId = reader.ReadNetworkId();
			this.authorityState = reader.ReadBoolean();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)this.connectionId);
			writer.Write(this.netId);
			writer.Write(this.authorityState);
		}

		public int connectionId;

		public NetworkInstanceId netId;

		public bool authorityState;
	}
}