using HarmonyLib;
using QSB.Patches;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(ProfileManagerUpdater))]
public class ProfileManagerUpdaterPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ProfileManagerUpdater.Start))]
	public static bool Start(ProfileManagerUpdater __instance)
	{
		__instance._profileManager = QSBCore.ProfileManager;
		__instance.enabled = true;
		return false;
	}
}
