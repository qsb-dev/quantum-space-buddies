using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.OrbSync.Events
{
	public class OrbSlotMessage : WorldObjectMessage
	{
		public int OrbId { get; set; }
		public bool SlotState { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			OrbId = reader.ReadInt32();
			SlotState = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(OrbId);
			writer.Write(SlotState);
		}
	}
}
