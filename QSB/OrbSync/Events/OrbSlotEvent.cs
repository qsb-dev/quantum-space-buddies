using QSB.Events;
using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.OrbSync.Events
{
	public class OrbSlotEvent : QSBEvent<OrbSlotMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<QSBOrbSlot, QSBOrb, bool>.AddListener(EventNames.QSBOrbSlot, Handler);
		public override void CloseListener() => GlobalMessenger<QSBOrbSlot, QSBOrb, bool>.RemoveListener(EventNames.QSBOrbSlot, Handler);

		private void Handler(QSBOrbSlot qsbOrbSlot, QSBOrb qsbOrb, bool slotState) => SendEvent(CreateMessage(qsbOrbSlot, qsbOrb, slotState));

		private OrbSlotMessage CreateMessage(QSBOrbSlot qsbOrbSlot, QSBOrb qsbOrb, bool slotState) => new()
		{
			ObjectId = qsbOrbSlot.ObjectId,
			OrbId = qsbOrb.ObjectId,
			State = slotState
		};

		public override void OnReceiveRemote(bool server, OrbSlotMessage message)
		{
			var qsbOrbSlot = QSBWorldSync.GetWorldFromId<QSBOrbSlot>(message.ObjectId);
			var qsbOrb = QSBWorldSync.GetWorldFromId<QSBOrb>(message.OrbId);
			qsbOrbSlot.SetState(qsbOrb, message.State);
		}
	}
}