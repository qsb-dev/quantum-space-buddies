using QSB.Events;
using QSB.GeyserSync.WorldObjects;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.GeyserSync.Events
{
	public class GeyserEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override void SetupListener() => GlobalMessenger<int, bool>.AddListener(EventNames.QSBGeyserState, Handler);
		public override void CloseListener() => GlobalMessenger<int, bool>.RemoveListener(EventNames.QSBGeyserState, Handler);

		private void Handler(int id, bool state) => SendEvent(CreateMessage(id, state));

		private BoolWorldObjectMessage CreateMessage(int id, bool state) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = id,
			State = state
		};

		public override void OnReceiveRemote(bool isHost, BoolWorldObjectMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var geyser = QSBWorldSync.GetWorldFromId<QSBGeyser>(message.ObjectId);
			geyser?.SetState(message.State);
		}
	}
}