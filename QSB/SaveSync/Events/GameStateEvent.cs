using QSB.Events;
using QSB.Menus;
using QSB.Utility;
using System;
using System.Linq;

namespace QSB.SaveSync.Events
{
	// only to be sent from host
	internal class GameStateEvent : QSBEvent<GameStateMessage>
	{
		public override bool RequireWorldObjectsReady => false;

		public override void SetupListener() => GlobalMessenger.AddListener(EventNames.QSBGameDetails, Handler);
		public override void CloseListener() => GlobalMessenger.RemoveListener(EventNames.QSBGameDetails, Handler);

		private void Handler() => SendEvent(CreateMessage());

		private GameStateMessage CreateMessage() => new()
		{
			AboutId = LocalPlayerId,
			InSolarSystem = QSBSceneManager.CurrentScene == OWScene.SolarSystem,
			InEye = QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse,
			LoopCount = StandaloneProfileManager.SharedInstance.currentProfileGameSave.loopCount,
			KnownFrequencies = StandaloneProfileManager.SharedInstance.currentProfileGameSave.knownFrequencies
		};

		public override void OnReceiveRemote(bool isHost, GameStateMessage message)
		{
			var gameSave = StandaloneProfileManager.SharedInstance.currentProfileGameSave;
			DebugLog.DebugWrite($"loopcount:{message.LoopCount}");
			gameSave.loopCount = message.LoopCount;
			for (var i = 0; i < message.KnownFrequencies.Length; i++)
			{
				DebugLog.DebugWrite($"knowsFrequency{i}:{message.KnownFrequencies[i]}");
			}
			gameSave.knownFrequencies = message.KnownFrequencies;

			PlayerData.SaveCurrentGame();

			DebugLog.DebugWrite($"inEye:{message.InEye}");
			DebugLog.DebugWrite($"inSolarSystem:{message.InSolarSystem}");
			if (message.InEye != (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
				|| message.InSolarSystem != (QSBSceneManager.CurrentScene == OWScene.SolarSystem))
			{
				MenuManager.Instance.JoinGame(message.InEye, message.InSolarSystem);
			}
		}
	}
}
