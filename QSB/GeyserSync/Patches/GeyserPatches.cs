using HarmonyLib;
using QSB.Patches;

namespace QSB.GeyserSync.Patches;

[HarmonyPatch]
public class GeyserPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GeyserController), nameof(GeyserController.Update))]
	public static bool Empty()
		=> false;
}