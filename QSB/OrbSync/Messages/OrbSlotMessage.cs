using Mirror;
using QSB.Messaging;
using QSB.OrbSync.WorldObjects;

namespace QSB.OrbSync.Messages
{
	public class OrbSlotMessage : QSBWorldObjectMessage<QSBOrb>
	{
		private int SlotIndex;

		public OrbSlotMessage(int slotIndex) => SlotIndex = slotIndex;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SlotIndex);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			SlotIndex = reader.Read<int>();
		}

		public override void OnReceiveRemote() => WorldObject.SetSlot(SlotIndex);
	}
}