namespace QSB.QuantumUNET
{
	public class QSBPeerListMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader)
		{
			this.oldServerConnectionId = (int)reader.ReadPackedUInt32();
			int num = (int)reader.ReadUInt16();
			this.peers = new QSBPeerInfoMessage[num];
			for (int i = 0; i < this.peers.Length; i++)
			{
				QSBPeerInfoMessage peerInfoMessage = new QSBPeerInfoMessage();
				peerInfoMessage.Deserialize(reader);
				this.peers[i] = peerInfoMessage;
			}
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)this.oldServerConnectionId);
			writer.Write((ushort)this.peers.Length);
			for (int i = 0; i < this.peers.Length; i++)
			{
				this.peers[i].Serialize(writer);
			}
		}

		public QSBPeerInfoMessage[] peers;

		public int oldServerConnectionId;
	}
}