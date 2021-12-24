using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.ItemSync.Messages
{
	public class SocketItemMessage : PlayerMessage
	{
		public int SocketId { get; set; }
		public int ItemId { get; set; }
		public SocketEventType SocketType { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			SocketId = reader.ReadInt32();
			ItemId = reader.ReadInt32();
			SocketType = (SocketEventType)reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SocketId);
			writer.Write(ItemId);
			writer.Write((int)SocketType);
		}
	}
}
