using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	public class MeteorLaunchEvent : QSBEvent<MeteorLaunchMessage>
	{
		public override EventType Type => EventType.MeteorLaunch;

		public override void SetupListener()
			=> GlobalMessenger<int, float>.AddListener(EventNames.QSBMeteorLaunch, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int, float>.RemoveListener(EventNames.QSBMeteorLaunch, Handler);

		private void Handler(int id, float launchSpeed) => SendEvent(CreateMessage(id, launchSpeed));

		private MeteorLaunchMessage CreateMessage(int id, float launchSpeed) => new MeteorLaunchMessage
		{
			ObjectId = id,
			LaunchSpeed = launchSpeed
		};

		public override void OnReceiveRemote(bool isHost, MeteorLaunchMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var qsbMeteorLauncher = QSBWorldSync.GetWorldFromId<QSBMeteorLauncher>(message.ObjectId);
			qsbMeteorLauncher.LaunchMeteor(message.LaunchSpeed);
		}
	}
}
