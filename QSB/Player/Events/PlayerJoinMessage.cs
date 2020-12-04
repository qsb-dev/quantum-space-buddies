using QSB.Messaging;
using QSB.QuantumUNET;

namespace QSB.Player.Events
{
	public class PlayerJoinMessage : PlayerMessage
	{
		public string PlayerName { get; set; }

		public override void Deserialize(QSBNetworkReader reader)
		{
			base.Deserialize(reader);
			PlayerName = reader.ReadString();
		}

		public override void Serialize(QSBNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(PlayerName);
		}
	}
}