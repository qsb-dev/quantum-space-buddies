using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.MeteorSync.Events
{
	public class MeteorLaunchEvent : QSBEvent<MeteorLaunchMessage>
	{
		public override EventType Type => EventType.MeteorLaunch;

		public override void SetupListener()
			=> GlobalMessenger<QSBMeteorLauncher>.AddListener(EventNames.QSBMeteorLaunch, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBMeteorLauncher>.RemoveListener(EventNames.QSBMeteorLaunch, Handler);

		private void Handler(QSBMeteorLauncher qsbMeteorLauncher) =>
			SendEvent(CreateMessage(qsbMeteorLauncher));

		private MeteorLaunchMessage CreateMessage(QSBMeteorLauncher qsbMeteorLauncher) => new()
		{
			ObjectId = qsbMeteorLauncher.ObjectId,
			MeteorId = qsbMeteorLauncher.MeteorId,
			LaunchSpeed = qsbMeteorLauncher.LaunchSpeed,
		};

		public override void OnReceiveRemote(bool isHost, MeteorLaunchMessage message)
		{
			if (!MeteorManager.Ready)
			{
				return;
			}

			var qsbMeteorLauncher = QSBWorldSync.GetWorldFromId<QSBMeteorLauncher>(message.ObjectId);
			qsbMeteorLauncher.LaunchMeteor(message.MeteorId, message.LaunchSpeed);
		}
	}
}
