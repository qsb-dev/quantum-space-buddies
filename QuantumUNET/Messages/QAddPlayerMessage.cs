using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	internal class QAddPlayerMessage : QMessageBase
	{
		public short playerControllerId;
		public int msgSize;
		public byte[] msgData;

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(playerControllerId);
			writer.WriteBytesAndSize(msgData, msgSize);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			playerControllerId = reader.ReadInt16();
			msgData = reader.ReadBytesAndSize();
			msgSize = msgData?.Length ?? 0;
		}
	}
}