using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.MeteorSync.Events
{
	public class MeteorPreLaunchEvent : QSBEvent<WorldObjectMessage>
	{
		public override void SetupListener()
			=> GlobalMessenger<QSBMeteorLauncher>.AddListener(EventNames.QSBMeteorPreLaunch, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBMeteorLauncher>.RemoveListener(EventNames.QSBMeteorPreLaunch, Handler);

		private void Handler(QSBMeteorLauncher qsbMeteorLauncher) => SendEvent(CreateMessage(qsbMeteorLauncher));

		private WorldObjectMessage CreateMessage(QSBMeteorLauncher qsbMeteorLauncher) => new()
		{
			ObjectId = qsbMeteorLauncher.ObjectId
		};

		public override void OnReceiveRemote(bool isHost, WorldObjectMessage message)
		{
			if (!MeteorManager.Ready)
			{
				return;
			}

			var qsbMeteorLauncher = QSBWorldSync.GetWorldFromId<QSBMeteorLauncher>(message.ObjectId);
			qsbMeteorLauncher.PreLaunchMeteor();
		}
	}
}
