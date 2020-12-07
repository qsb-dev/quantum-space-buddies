namespace QuantumUNET
{
	internal class QSBAddPlayerMessage : QSBMessageBase
	{
		public short playerControllerId;
		public int msgSize;
		public byte[] msgData;

		public override void Deserialize(QSBNetworkReader reader)
		{
			playerControllerId = reader.ReadInt16();
			msgData = reader.ReadBytesAndSize();
			if (msgData == null)
			{
				msgSize = 0;
			}
			else
			{
				msgSize = msgData.Length;
			}
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write(playerControllerId);
			writer.WriteBytesAndSize(msgData, msgSize);
		}
	}
}