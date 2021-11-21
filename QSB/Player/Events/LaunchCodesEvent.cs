using QSB.Events;
using QSB.Messaging;

namespace QSB.Player.Events
{
	internal class LaunchCodesEvent : QSBEvent<PlayerMessage>
	{
		public override EventType Type => EventType.LaunchCodes;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.LaunchCodes, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.LaunchCodes, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new PlayerMessage
		{
			AboutId = LocalPlayerId
		};

		public override void OnReceiveRemote(bool isHost, PlayerMessage message)
		{
			var flag = false;
			if (!PlayerData._currentGameSave.PersistentConditionExists("LAUNCH_CODES_GIVEN"))
			{
				flag = true;
			}

			else if (PlayerData._currentGameSave.GetPersistentCondition("LAUNCH_CODES_GIVEN"))
			{
				flag = true;
			}

			if (flag)
			{
				PlayerData._currentGameSave.SetPersistentCondition("LAUNCH_CODES_GIVEN", true);
			}
		}
	}
}
