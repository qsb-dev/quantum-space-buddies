using QSB.Events;
using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.OrbSync.Events
{
	public class OrbSlottedEvent : QSBEvent<OrbSlottedMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<QSBOrb, int, bool>.AddListener(EventNames.QSBOrbSlotted, Handler);
		public override void CloseListener() => GlobalMessenger<QSBOrb, int, bool>.RemoveListener(EventNames.QSBOrbSlotted, Handler);

		private void Handler(QSBOrb qsbOrb, int slotIndex, bool slotted) => SendEvent(CreateMessage(qsbOrb, slotIndex, slotted));

		private OrbSlottedMessage CreateMessage(QSBOrb qsbOrb, int slotIndex, bool slotted) => new()
		{
			ObjectId = qsbOrb.ObjectId,
			SlotIndex = slotIndex,
			Slotted = slotted
		};

		public override void OnReceiveRemote(bool server, OrbSlottedMessage message)
		{
			var qsbOrb = QSBWorldSync.GetWorldFromId<QSBOrb>(message.ObjectId);
			qsbOrb.SetSlotted(message.SlotIndex, message.Slotted);
		}
	}
}
