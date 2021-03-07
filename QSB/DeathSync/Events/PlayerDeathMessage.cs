using QSB.Messaging;
using QuantumUNET.Transport;

namespace QSB.DeathSync.Events
{
	public class PlayerDeathMessage : PlayerMessage
	{
		public DeathType DeathType { get; set; }
		public int NecronomiconIndex { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			DeathType = (DeathType)reader.ReadInt32();
			NecronomiconIndex = reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write((int)DeathType);
			writer.Write(NecronomiconIndex);
		}
	}
}
