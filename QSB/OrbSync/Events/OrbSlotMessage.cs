using QSB.Messaging;
using QSB.OrbSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.OrbSync.Events
{
	public class OrbSlotMessage : QSBWorldObjectMessage<QSBOrbSlot>
	{
		public int OrbId;
		public bool SlotState;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(OrbId);
			writer.Write(SlotState);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			OrbId = reader.ReadInt32();
			SlotState = reader.ReadBoolean();
		}

		public override void OnReceiveRemote() => WorldObject.SetState(SlotState, OrbId);
	}
}
