using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QSB.WorldSync.Events;
using UnityEngine;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	public class MeteorPreLaunchEvent : QSBEvent<WorldObjectMessage>
	{
		public override EventType Type => EventType.MeteorPreLaunch;

		public override void SetupListener()
			=> GlobalMessenger<int>.AddListener(EventNames.QSBMeteorPreLaunch, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int>.RemoveListener(EventNames.QSBMeteorPreLaunch, Handler);

		private void Handler(int id) => SendEvent(CreateMessage(id));

		private WorldObjectMessage CreateMessage(int id) => new WorldObjectMessage
		{
			ObjectId = id
		};

		public override void OnReceiveRemote(bool isHost, WorldObjectMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var qsbMeteorLauncher = QSBWorldSync.GetWorldFromId<QSBMeteorLauncher>(message.ObjectId);
			qsbMeteorLauncher.PreLaunchMeteor();
		}
	}
}
