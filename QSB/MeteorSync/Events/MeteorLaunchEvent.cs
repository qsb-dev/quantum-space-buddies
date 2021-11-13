using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	public class MeteorLaunchEvent : QSBEvent<MeteorLaunchMessage>
	{
		public override EventType Type => EventType.MeteorLaunch;

		public override void SetupListener()
			=> GlobalMessenger<int, int, float>.AddListener(EventNames.QSBMeteorLaunch, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int, int, float>.RemoveListener(EventNames.QSBMeteorLaunch, Handler);

		private void Handler(int id, int meteorId, float launchSpeed) =>
			SendEvent(CreateMessage(id, meteorId, launchSpeed));

		private MeteorLaunchMessage CreateMessage(int id, int meteorId, float launchSpeed) => new MeteorLaunchMessage
		{
			ObjectId = id,
			MeteorId = meteorId,
			LaunchSpeed = launchSpeed
		};

		public override void OnReceiveRemote(bool isHost, MeteorLaunchMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var qsbMeteorLauncher = QSBWorldSync.GetWorldFromId<QSBMeteorLauncher>(message.ObjectId);
			qsbMeteorLauncher.LaunchMeteor(message.MeteorId, message.LaunchSpeed);
		}
	}
}
