using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.OrbSync.Events
{
	public class OrbSlottedMessage : WorldObjectMessage
	{
		public int SlotIndex;
		public bool Slotted;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			SlotIndex = reader.ReadInt32();
			Slotted = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SlotIndex);
			writer.Write(Slotted);
		}
	}
}
