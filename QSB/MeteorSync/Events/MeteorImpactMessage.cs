using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.MeteorSync.Events
{
	public class MeteorImpactMessage : WorldObjectMessage
	{
		public float Damage;

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
