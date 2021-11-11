using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	public class MeteorImpactEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.MeteorImpact;

		public override void SetupListener()
			=> GlobalMessenger<QSBMeteor>.AddListener(EventNames.QSBMeteorImpact, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBMeteor>.RemoveListener(EventNames.QSBMeteorImpact, Handler);

		private void Handler(QSBMeteor qsbMeteor) => SendEvent(CreateMessage(qsbMeteor));

		private WorldObjectMessage CreateMessage(QSBMeteor qsbMeteor) => new WorldObjectMessage
		{
			ObjectId = qsbMeteor.ObjectId
			// todo where
			// todo velocity
		};

		public override void OnReceiveRemote(bool isHost, WorldObjectMessage message)
		{
			var qsbMeteor = QSBWorldSync.GetWorldFromId<QSBMeteor>(message.ObjectId);
			// todo
		}
	}
}
