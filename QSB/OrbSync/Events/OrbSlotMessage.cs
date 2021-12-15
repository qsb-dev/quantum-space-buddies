using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.OrbSync.Events
{
	public class OrbSlotMessage : BoolWorldObjectMessage
	{
		public int OrbId { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			OrbId = reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(OrbId);
		}
	}
}
