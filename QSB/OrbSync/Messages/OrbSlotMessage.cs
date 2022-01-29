using Mirror;
using QSB.Messaging;
using QSB.OrbSync.WorldObjects;

namespace QSB.OrbSync.Messages
{
	public class OrbSlotMessage : QSBWorldObjectMessage<QSBOrb, int>
	{
		public OrbSlotMessage(int slotIndex) => Value = slotIndex;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			Value = reader.Read<int>();
		}

		public override void OnReceiveRemote() => WorldObject.SetSlot(Value);
	}
}