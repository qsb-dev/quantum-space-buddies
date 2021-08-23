using QuantumUNET.Transport;

namespace QSB.ShipSync.Events.Hull
{
	public class HullImpactMessage : ImpactDataMessage
	{
		public float Damage { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Damage = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Damage);
		}
	}
}
