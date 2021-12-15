using QSB.Events;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.OrbSync.Events
{
	public class OrbSlotEvent : QSBEvent<OrbSlotMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<QSBOrb, NomaiInterfaceSlot, bool>.AddListener(EventNames.QSBOrbSlot, Handler);
		public override void CloseListener() => GlobalMessenger<QSBOrb, NomaiInterfaceSlot, bool>.RemoveListener(EventNames.QSBOrbSlot, Handler);

		private void Handler(QSBOrb qsbOrb, NomaiInterfaceSlot slot, bool playAudio) => SendEvent(CreateMessage(qsbOrb, slot, playAudio));

		private OrbSlotMessage CreateMessage(QSBOrb qsbOrb, NomaiInterfaceSlot slot, bool playAudio) => new()
		{
			ObjectId = qsbOrb.ObjectId,
			SlotIndex = qsbOrb.AttachedObject._slots.IndexOf(slot),
			PlayAudio = playAudio
		};

		public override void OnReceiveRemote(bool server, OrbSlotMessage message)
		{
			var qsbOrb = QSBWorldSync.GetWorldFromId<QSBOrb>(message.ObjectId);
			qsbOrb.SetOccupiedSlot(message.SlotIndex == -1 ? null : qsbOrb.AttachedObject._slots[message.SlotIndex], message.PlayAudio);
		}
	}
}
