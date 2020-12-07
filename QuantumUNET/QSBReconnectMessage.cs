using UnityEngine.Networking;

namespace QuantumUNET
{
	public class QSBReconnectMessage : QSBMessageBase
	{
		public int oldConnectionId;
		public short playerControllerId;
		public NetworkInstanceId netId;
		public int msgSize;
		public byte[] msgData;

		public override void Deserialize(QSBNetworkReader reader)
		{
			oldConnectionId = (int)reader.ReadPackedUInt32();
			playerControllerId = (short)reader.ReadPackedUInt32();
			netId = reader.ReadNetworkId();
			msgData = reader.ReadBytesAndSize();
			msgSize = msgData.Length;
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)oldConnectionId);
			writer.WritePackedUInt32((uint)playerControllerId);
			writer.Write(netId);
			writer.WriteBytesAndSize(msgData, msgSize);
		}
	}
}