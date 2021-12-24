using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.ShipSync.Messages.Hull
{
	public class HullChangeIntegrityMessage : WorldObjectMessage
	{
		public float Integrity { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Integrity = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Integrity);
		}
	}
}
