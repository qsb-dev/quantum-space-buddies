using OWML.Common;
using QSB.Menus;
using QSB.Messaging;
using QSB.Utility;
using QuantumUNET.Transport;
using System.Collections.Generic;

namespace QSB.SaveSync.Messages
{
	/// <summary>
	/// always sent by host
	/// </summary>
	internal class GameStateMessage : QSBMessage
	{
		private bool WarpedToTheEye;
		private float SecondsRemainingOnWarp;
		private bool LaunchCodesGiven;
		private int LoopCount;
		private bool[] KnownFrequencies;
		private Dictionary<int, bool> KnownSignals;

		public GameStateMessage(uint toId)
		{
			To = toId;
			var gameSave = StandaloneProfileManager.SharedInstance.currentProfileGameSave;
			WarpedToTheEye = gameSave.warpedToTheEye;
			SecondsRemainingOnWarp = gameSave.secondsRemainingOnWarp;
			LaunchCodesGiven = PlayerData.KnowsLaunchCodes();
			LoopCount = gameSave.loopCount;
			KnownFrequencies = gameSave.knownFrequencies;
			KnownSignals = gameSave.knownSignals;
		}


		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(WarpedToTheEye);
			writer.Write(SecondsRemainingOnWarp);
			writer.Write(LaunchCodesGiven);
			writer.Write(LoopCount);

			writer.Write(KnownFrequencies.Length);
			foreach (var item in KnownFrequencies)
			{
				writer.Write(item);
			}

			writer.Write(KnownSignals.Count);
			foreach (var (name, discovered) in KnownSignals)
			{
				writer.Write(name);
				writer.Write(discovered);
			}
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			WarpedToTheEye = reader.ReadBoolean();
			SecondsRemainingOnWarp = reader.ReadSingle();
			LaunchCodesGiven = reader.ReadBoolean();
			LoopCount = reader.ReadInt32();

			var frequenciesLength = reader.ReadInt32();
			KnownFrequencies = new bool[frequenciesLength];
			for (var i = 0; i < frequenciesLength; i++)
			{
				KnownFrequencies[i] = reader.ReadBoolean();
			}

			var signalsLength = reader.ReadInt32();
			KnownSignals = new Dictionary<int, bool>(signalsLength);
			for (var i = 0; i < signalsLength; i++)
			{
				var key = reader.ReadInt32();
				var value = reader.ReadBoolean();
				KnownSignals.Add(key, value);
			}
		}

		public override void OnReceiveRemote()
		{
			if (QSBSceneManager.CurrentScene != OWScene.TitleScreen)
			{
				DebugLog.ToConsole($"Error - Tried to handle GameStateEvent when not in TitleScreen!", MessageType.Error);
				return;
			}

			PlayerData.ResetGame();

			var gameSave = StandaloneProfileManager.SharedInstance.currentProfileGameSave;
			gameSave.loopCount = LoopCount;
			gameSave.knownFrequencies = KnownFrequencies;
			gameSave.knownSignals = KnownSignals;
			gameSave.warpedToTheEye = WarpedToTheEye;
			gameSave.secondsRemainingOnWarp = SecondsRemainingOnWarp;

			PlayerData.SetPersistentCondition("LAUNCH_CODES_GIVEN", LaunchCodesGiven);

			PlayerData.SaveCurrentGame();

			MenuManager.Instance.JoinGame(WarpedToTheEye);
		}
	}
}