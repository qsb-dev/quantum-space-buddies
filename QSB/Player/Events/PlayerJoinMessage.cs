using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.Player.Events
{
	public class PlayerJoinMessage : PlayerMessage
	{
		public string PlayerName { get; set; }
		public string QSBVersion { get; set; }
		public string GameVersion { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerName = reader.ReadString();
			QSBVersion = reader.ReadString();
			GameVersion = reader.ReadString();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerName);
			writer.Write(QSBVersion);
			writer.Write(GameVersion);
		}
	}
}