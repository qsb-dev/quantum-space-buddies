using QuantumUNET.Messages;
using QuantumUNET.Transport;

namespace QSB.Messaging
{
	public class PlayerMessage : QMessageBase
	{
		public uint FromId { get; set; }
		public uint AboutId { get; set; }
		public bool OnlySendToServer { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			FromId = reader.ReadUInt32();
			AboutId = reader.ReadUInt32();
			OnlySendToServer = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(FromId);
			writer.Write(AboutId);
			writer.Write(OnlySendToServer);
		}
	}
}