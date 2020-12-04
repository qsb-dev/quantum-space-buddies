namespace QSB.QuantumUNET
{
	internal class QSBAddPlayerMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader)
		{
			this.playerControllerId = (short)reader.ReadUInt16();
			this.msgData = reader.ReadBytesAndSize();
			if (this.msgData == null)
			{
				this.msgSize = 0;
			}
			else
			{
				this.msgSize = this.msgData.Length;
			}
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.Write((ushort)this.playerControllerId);
			writer.WriteBytesAndSize(this.msgData, this.msgSize);
		}

		public short playerControllerId;

		public int msgSize;

		public byte[] msgData;
	}
}