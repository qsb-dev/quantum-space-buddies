using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QSB.WorldSync.Events;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	/// called when we request a resync on client join
	public class MeteorResyncEvent : QSBEvent<MeteorResyncMessage>
	{
		public override EventType Type => EventType.MeteorResync;

		public override void SetupListener()
			=> GlobalMessenger<QSBMeteor>.AddListener(EventNames.QSBMeteorResync, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBMeteor>.RemoveListener(EventNames.QSBMeteorResync, Handler);

		private void Handler(QSBMeteor qsbMeteor) => SendEvent(CreateMessage(qsbMeteor));

		private MeteorResyncMessage CreateMessage(QSBMeteor qsbMeteor) => new MeteorResyncMessage
		{
			ObjectId = qsbMeteor.ObjectId
			// todo pos/rot/vel/angvel
			// todo is suspended/launched
			// todo fragment states
		};

		public override void OnReceiveRemote(bool isHost, MeteorResyncMessage message)
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
