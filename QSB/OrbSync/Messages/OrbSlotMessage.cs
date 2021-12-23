using QSB.Messaging;
using QSB.OrbSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.OrbSync.Messages
{
	public class OrbSlotMessage : QSBWorldObjectMessage<QSBOrb>
	{
		private int SlotIndex;

		public OrbSlotMessage(int slotIndex) => SlotIndex = slotIndex;

		public OrbSlotMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SlotIndex);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			SlotIndex = reader.ReadInt32();
		}

		public override void OnReceiveRemote() => WorldObject.SetSlot(SlotIndex);
	}
}