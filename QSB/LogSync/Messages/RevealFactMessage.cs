using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.LogSync.Messages
{
	public class RevealFactMessage : PlayerMessage
	{
		public string FactId { get; set; }
		public bool SaveGame { get; set; }
		public bool ShowNotification { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			FactId = reader.ReadString();
			SaveGame = reader.ReadBoolean();
			ShowNotification = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(FactId);
			writer.Write(SaveGame);
			writer.Write(ShowNotification);
		}
	}
}
