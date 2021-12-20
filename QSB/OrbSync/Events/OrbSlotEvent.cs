using QSB.Events;
using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.OrbSync.Events
{
	public class OrbSlotEvent : QSBEvent<OrbSlotMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<QSBOrb, int>.AddListener(EventNames.QSBOrbSlot, Handler);
		public override void CloseListener() => GlobalMessenger<QSBOrb, int>.RemoveListener(EventNames.QSBOrbSlot, Handler);

		private void Handler(QSBOrb qsbOrb, int slotIndex) => SendEvent(CreateMessage(qsbOrb, slotIndex));

		private OrbSlotMessage CreateMessage(QSBOrb qsbOrb, int slotIndex) => new()
		{
			ObjectId = qsbOrb.ObjectId,
			SlotIndex = slotIndex
		};

		public override void OnReceiveRemote(bool server, OrbSlotMessage message)
		{
			var qsbOrb = QSBWorldSync.GetWorldFromId<QSBOrb>(message.ObjectId);
			qsbOrb.SetSlot(message.SlotIndex);
		}
	}
}
