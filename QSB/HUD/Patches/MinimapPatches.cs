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
		if (__instance._minimapMode == Minimap.MinimapMode.Player)
		{
			MultiplayerHUDManager.Instance.UpdateMinimapMarkers(__instance);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(Minimap.HideMinimap))]
	public static void HideMinimap(Minimap __instance)
	{
		if (__instance._minimapMode == Minimap.MinimapMode.Player)
		{
			MultiplayerHUDManager.Instance.HideMinimap(__instance);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(Minimap.ShowMinimap))]
	public static void ShowMinimap(Minimap __instance)
	{
		if (__instance._minimapMode == Minimap.MinimapMode.Player)
		{
			MultiplayerHUDManager.Instance.ShowMinimap(__instance);
		}
	}
}
