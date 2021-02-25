using QSB.Messaging;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.ItemSync.Events
{
	public class SocketItemMessage : PlayerMessage
	{
		public int SocketId { get; set; }
		public int ItemId { get; set; }
		public bool Inserting { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			SocketId = reader.ReadInt32();
			ItemId = reader.ReadInt32();
			Inserting = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SocketId);
			writer.Write(ItemId);
			writer.Write(Inserting);
		}
	}
}
