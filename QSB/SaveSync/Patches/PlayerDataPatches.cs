using HarmonyLib;
using QSB.Patches;
using UnityEngine;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(PlayerData))]
public class PlayerDataPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerData.ResetGame))]
	public static bool ResetGame()
	{
		PlayerData._currentGameSave = new GameSave();
		QSBCore.ProfileManager.SaveGame(PlayerData._currentGameSave, null, null, null);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerData.SaveCurrentGame))]
	public static bool SaveCurrentGame()
	{
		PlayerData._currentGameSave.version = Application.version;
		QSBCore.ProfileManager.SaveGame(PlayerData._currentGameSave, null, null, null);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerData.SaveInputSettings))]
	public static bool SaveInputSettings()
	{
		QSBCore.ProfileManager.SaveGame(null, null, null, PlayerData.inputJSON);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerData.SaveSettings))]
	public static bool SaveSettings()
	{
		QSBCore.ProfileManager.SaveGame(null, PlayerData._settingsSave, PlayerData._graphicsSettings, PlayerData.inputJSON);
		return false;
	}

	// this is actually still StandaloneProfileManager in the gamepass dll. game bug?
	[HarmonyPrefix]
	[HarmonyPatch(nameof(PlayerData.IsBusy))]
	public static bool IsBusy(ref bool __result)
	{
		__result = QSBCore.ProfileManager.isBusyWithFileOps;
		return false;
	}
}
