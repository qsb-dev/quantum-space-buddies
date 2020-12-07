using UnityEngine.Networking;

namespace QuantumUNET
{
	public class QSBReconnectMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader)
		{
			this.oldConnectionId = (int)reader.ReadPackedUInt32();
			this.playerControllerId = (short)reader.ReadPackedUInt32();
			this.netId = reader.ReadNetworkId();
			this.msgData = reader.ReadBytesAndSize();
			this.msgSize = this.msgData.Length;
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)this.oldConnectionId);
			writer.WritePackedUInt32((uint)this.playerControllerId);
			writer.Write(this.netId);
			writer.WriteBytesAndSize(this.msgData, this.msgSize);
		}

		public int oldConnectionId;

		public short playerControllerId;

		public NetworkInstanceId netId;

		public int msgSize;

		public byte[] msgData;
	}
}