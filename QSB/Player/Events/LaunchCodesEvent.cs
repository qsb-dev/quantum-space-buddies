using QSB.Events;
using QSB.Messaging;

namespace QSB.Player.Events
{
	internal class LaunchCodesEvent : QSBEvent<PlayerMessage>
	{
		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBLearnLaunchCodes, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBLearnLaunchCodes, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private PlayerMessage CreateMessage() => new()
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
				DialogueConditionManager.SharedInstance.SetConditionState("SCIENTIST_3", true);
				PlayerData._currentGameSave.SetPersistentCondition("LAUNCH_CODES_GIVEN", true);
				GlobalMessenger.FireEvent("LearnLaunchCodes");
			}
		}
	}
}
