using QSB.Events;
using QSB.Menus;
using QSB.Utility;

namespace QSB.SaveSync.Events
{
	// only to be sent from host
	internal class GameStateEvent : QSBEvent<GameStateMessage>
	{
		public override bool RequireWorldObjectsReady => false;

		public override void SetupListener() => GlobalMessenger<uint>.AddListener(EventNames.QSBGameDetails, Handler);
		public override void CloseListener() => GlobalMessenger<uint>.RemoveListener(EventNames.QSBGameDetails, Handler);

		private void Handler(uint toId) => SendEvent(CreateMessage(toId));

		private GameStateMessage CreateMessage(uint toId) => new()
		{
			AboutId = LocalPlayerId,
			ForId = toId,
			InSolarSystem = QSBSceneManager.CurrentScene == OWScene.SolarSystem,
			InEye = QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse,
			LaunchCodesGiven = PlayerData.KnowsLaunchCodes(),
			LoopCount = StandaloneProfileManager.SharedInstance.currentProfileGameSave.loopCount,
			KnownFrequencies = StandaloneProfileManager.SharedInstance.currentProfileGameSave.knownFrequencies,
			KnownSignals = StandaloneProfileManager.SharedInstance.currentProfileGameSave.knownSignals
		};

		public override void OnReceiveRemote(bool isHost, GameStateMessage message)
		{
			if (QSBSceneManager.CurrentScene != OWScene.TitleScreen)
			{
				DebugLog.ToConsole($"Error - Treid to handle GameStateEvent when not in TitleScreen!", OWML.Common.MessageType.Error);
				return;
			}

			PlayerData.ResetGame();

			var gameSave = StandaloneProfileManager.SharedInstance.currentProfileGameSave;
			gameSave.loopCount = message.LoopCount;
			gameSave.knownFrequencies = message.KnownFrequencies;
			gameSave.knownSignals = message.KnownSignals;

			PlayerData.SetPersistentCondition("LAUNCH_CODES_GIVEN", message.LaunchCodesGiven);

			PlayerData.SaveCurrentGame();

			if (message.InEye != (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
				|| message.InSolarSystem != (QSBSceneManager.CurrentScene == OWScene.SolarSystem))
			{
				MenuManager.Instance.JoinGame(message.InEye, message.InSolarSystem);
			}
		}
	}
}
