using System.Collections.Generic;

namespace QSB.QuantumUNET
{
	public class QSBPeerInfoMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader)
		{
			this.connectionId = (int)reader.ReadPackedUInt32();
			this.address = reader.ReadString();
			this.port = (int)reader.ReadPackedUInt32();
			this.isHost = reader.ReadBoolean();
			this.isYou = reader.ReadBoolean();
			uint num = reader.ReadPackedUInt32();
			if (num > 0U)
			{
				List<QSBPeerInfoPlayer> list = new List<QSBPeerInfoPlayer>();
				for (uint num2 = 0U; num2 < num; num2 += 1U)
				{
					QSBPeerInfoPlayer item;
					item.netId = reader.ReadNetworkId();
					item.playerControllerId = (short)reader.ReadPackedUInt32();
					list.Add(item);
				}
				this.playerIds = list.ToArray();
			}
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)this.connectionId);
			writer.Write(this.address);
			writer.WritePackedUInt32((uint)this.port);
			writer.Write(this.isHost);
			writer.Write(this.isYou);
			if (this.playerIds == null)
			{
				writer.WritePackedUInt32(0U);
			}
			else
			{
				writer.WritePackedUInt32((uint)this.playerIds.Length);
				for (int i = 0; i < this.playerIds.Length; i++)
				{
					writer.Write(this.playerIds[i].netId);
					writer.WritePackedUInt32((uint)this.playerIds[i].playerControllerId);
				}
			}
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"PeerInfo conn:",
				this.connectionId,
				" addr:",
				this.address,
				":",
				this.port,
				" host:",
				this.isHost,
				" isYou:",
				this.isYou
			});
		}

		public int connectionId;

		public string address;

		public int port;

		public bool isHost;

		public bool isYou;

		public QSBPeerInfoPlayer[] playerIds;
	}
}