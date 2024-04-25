using HarmonyLib;
using NewHorizons.External;
using QSB;
using QSB.Patches;
using QSB.SaveSync;
using QSB.Utility;

namespace QSBNH.Patches;

/// <summary>
/// pretends to be a new profile when in multiplayer so nh saves its data to a new place
/// </summary>
public class NewHorizonsDataPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NewHorizonsData), nameof(NewHorizonsData.GetProfileName))]
	public static bool NewHorizonsData_GetProfileName(out string __result)
	{
		if (QSBCore.IsInMultiplayer)
		{
			__result = QSBStandaloneProfileManager.SharedInstance?.currentProfile?.profileName + "_mult";
			DebugLog.DebugWrite($"using fake multiplayer profile {__result} for NH");
		}
		else
		{
			__result = QSBStandaloneProfileManager.SharedInstance?.currentProfile?.profileName;
		}

		return false;
	}
}
