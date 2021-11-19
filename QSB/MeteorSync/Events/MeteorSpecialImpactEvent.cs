using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.MeteorSync.Events
{
	public class MeteorSpecialImpactEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.MeteorSpecialImpact;

		public override void SetupListener()
			=> GlobalMessenger<QSBMeteor>.AddListener(EventNames.QSBMeteorSpecialImpact, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBMeteor>.RemoveListener(EventNames.QSBMeteorSpecialImpact, Handler);

		private void Handler(QSBMeteor qsbMeteor) => SendEvent(CreateMessage(qsbMeteor));

		private WorldObjectMessage CreateMessage(QSBMeteor qsbMeteor) => new WorldObjectMessage
		{
			ObjectId = qsbMeteor.ObjectId
		};

		public override void OnReceiveRemote(bool isHost, WorldObjectMessage message)
		{
			if (!MeteorManager.Ready)
			{
				return;
			}

			var qsbMeteor = QSBWorldSync.GetWorldFromId<QSBMeteor>(message.ObjectId);
			qsbMeteor.SpecialImpact();
		}
	}
}
