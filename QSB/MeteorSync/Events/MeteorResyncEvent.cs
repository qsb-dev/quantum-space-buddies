using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QSB.WorldSync.Events;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	/// called when we request a resync on client join
	public class MeteorResyncEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.MeteorResync;

		public override void SetupListener()
			=> GlobalMessenger<QSBMeteor>.AddListener(EventNames.QSBMeteorResync, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBMeteor>.RemoveListener(EventNames.QSBMeteorResync, Handler);

		private void Handler(QSBMeteor qsbMeteor) => SendEvent(CreateMessage(qsbMeteor));

		private WorldObjectMessage CreateMessage(QSBMeteor qsbMeteor) => new WorldObjectMessage
		{
			ObjectId = qsbMeteor.ObjectId
			// todo is suspended
			// todo fragment states
		};

		public override void OnReceiveRemote(bool isHost, WorldObjectMessage message)
		{
			if (!MeteorManager.MeteorsReady)
			{
				return;
			}

			var qsbMeteor = QSBWorldSync.GetWorldFromId<QSBMeteor>(message.ObjectId);
			// todo

			DebugLog.DebugWrite($"{qsbMeteor.LogName} - resync requested");
		}
	}
}
