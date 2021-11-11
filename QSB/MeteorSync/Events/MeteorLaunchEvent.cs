using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	public class MeteorLaunchEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override EventType Type => EventType.MeteorLaunch;

		public override void SetupListener()
			=> GlobalMessenger<int, bool>.AddListener(EventNames.QSBMeteorLaunch, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int, bool>.RemoveListener(EventNames.QSBMeteorLaunch, Handler);

		private void Handler(int id, bool preLaunch) => SendEvent(CreateMessage(id, preLaunch));

		private BoolWorldObjectMessage CreateMessage(int id, bool preLaunch) => new BoolWorldObjectMessage
		{
			ObjectId = id,
			State = preLaunch
		};

		public override void OnReceiveRemote(bool isHost, BoolWorldObjectMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var qsbMeteorLauncher = QSBWorldSync.GetWorldFromId<QSBMeteorLauncher>(message.ObjectId);
			qsbMeteorLauncher.LaunchMeteor(message.State);
		}
	}
}
