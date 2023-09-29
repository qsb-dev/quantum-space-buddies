using Mirror;
using OWML.Common;
using QSB.Menus;
using QSB.Messaging;
using QSB.Utility;
using System.Collections.Generic;

namespace QSB.SaveSync.Messages;

/// <summary>
/// always sent by host
/// </summary>
public class GameStateMessage : QSBMessage
{
	private bool WarpedToTheEye;
	private float SecondsRemainingOnWarp;
	private bool LaunchCodesGiven;
	private int LoopCount;
	private bool[] KnownFrequencies;
	private Dictionary<int, bool> KnownSignals;
	private bool ReducedFrights;

	public GameStateMessage(uint toId)
	{
		To = toId;
		var gameSave = PlayerData._currentGameSave;
		WarpedToTheEye = gameSave.warpedToTheEye;
		SecondsRemainingOnWarp = gameSave.secondsRemainingOnWarp;
		LaunchCodesGiven = PlayerData.KnowsLaunchCodes();
		LoopCount = gameSave.loopCount;
		KnownFrequencies = gameSave.knownFrequencies;
		KnownSignals = gameSave.knownSignals;
		ReducedFrights = PlayerData.GetReducedFrights();
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(WarpedToTheEye);
		writer.Write(SecondsRemainingOnWarp);
		writer.Write(LaunchCodesGiven);
		writer.Write(LoopCount);

		writer.Write(KnownFrequencies);

		writer.Write(KnownSignals.Count);
		foreach (var (name, discovered) in KnownSignals)
		{
			writer.Write(name);
			writer.Write(discovered);
		}

		writer.Write(ReducedFrights);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		WarpedToTheEye = reader.Read<bool>();
		SecondsRemainingOnWarp = reader.Read<float>();
		LaunchCodesGiven = reader.Read<bool>();
		LoopCount = reader.Read<int>();

		KnownFrequencies = reader.Read<bool[]>();

		var signalsLength = reader.Read<int>();
		KnownSignals = new Dictionary<int, bool>(signalsLength);
		for (var i = 0; i < signalsLength; i++)
		{
			var key = reader.Read<int>();
			var value = reader.Read<bool>();
			KnownSignals.Add(key, value);
		}

		ReducedFrights = reader.Read<bool>();
	}

	public override void OnReceiveRemote()
	{
		if (QSBSceneManager.CurrentScene != OWScene.TitleScreen)
		{
			DebugLog.ToConsole($"Error - Tried to handle GameStateEvent when not in TitleScreen!", MessageType.Error);
			return;
		}

		PlayerData.ResetGame();

		var gameSave = PlayerData._currentGameSave;
		gameSave.loopCount = LoopCount;
		gameSave.knownFrequencies = KnownFrequencies;
		gameSave.knownSignals = KnownSignals;
		gameSave.warpedToTheEye = WarpedToTheEye;
		gameSave.secondsRemainingOnWarp = SecondsRemainingOnWarp;

		PlayerData.SetPersistentCondition("LAUNCH_CODES_GIVEN", LaunchCodesGiven);
		PlayerData._settingsSave.reducedFrights = ReducedFrights;

		PlayerData.SaveCurrentGame();

		MenuManager.Instance.LoadGame(WarpedToTheEye);
	}
}