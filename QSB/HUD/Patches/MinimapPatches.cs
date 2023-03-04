using HarmonyLib;
using QSB.Patches;

namespace QSB.HUD.Patches;

[HarmonyPatch(typeof(Minimap))]
internal class MinimapPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(Minimap.UpdateMarkers))]
	public static void UpdateMarkers(Minimap __instance)
	{
		MultiplayerHUDManager.Instance.UpdateMinimapMarkers(__instance);
	}
}
