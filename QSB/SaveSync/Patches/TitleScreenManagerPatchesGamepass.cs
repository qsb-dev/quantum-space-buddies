using HarmonyLib;
using QSB.Patches;
using System.Reflection;
using UnityEngine.UI;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(TitleScreenManager))]
internal class TitleScreenManagerPatchesGamepass : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;
	public override GameVendor PatchVendor => GameVendor.Gamepass;

	[HarmonyPrefix]
	[HarmonyPatch("SetUserAccountDisplayInfo")]
	public static bool SetUserAccountDisplayInfo(TitleScreenManager __instance)
	{
		var text = (Text)__instance.GetType().GetField("_gamertagDisplay", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
		text.text = ""; // no idea why, mobius be like
		text.text = QSBMSStoreProfileManager.SharedInstance.userDisplayName;
		return false;
	}
}
