using System.Collections.Generic;

namespace QuantumUNET.Messages
{
	public class QSBPeerInfoMessage : QSBMessageBase
	{
		public override void Deserialize(QSBNetworkReader reader)
		{
			connectionId = (int)reader.ReadPackedUInt32();
			address = reader.ReadString();
			port = (int)reader.ReadPackedUInt32();
			isHost = reader.ReadBoolean();
			isYou = reader.ReadBoolean();
			var num = reader.ReadPackedUInt32();
			if (num > 0U)
			{
				var list = new List<QSBPeerInfoPlayer>();
				for (var num2 = 0U; num2 < num; num2 += 1U)
				{
					QSBPeerInfoPlayer item;
					item.netId = reader.ReadNetworkId();
					item.playerControllerId = (short)reader.ReadPackedUInt32();
					list.Add(item);
				}
				playerIds = list.ToArray();
			}
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)connectionId);
			writer.Write(address);
			writer.WritePackedUInt32((uint)port);
			writer.Write(isHost);
			writer.Write(isYou);
			if (playerIds == null)
			{
				writer.WritePackedUInt32(0U);
			}
			else
			{
				writer.WritePackedUInt32((uint)playerIds.Length);
				for (var i = 0; i < playerIds.Length; i++)
				{
					writer.Write(playerIds[i].netId);
					writer.WritePackedUInt32((uint)playerIds[i].playerControllerId);
				}
			}
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"PeerInfo conn:",
				connectionId,
				" addr:",
				address,
				":",
				port,
				" host:",
				isHost,
				" isYou:",
				isYou
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