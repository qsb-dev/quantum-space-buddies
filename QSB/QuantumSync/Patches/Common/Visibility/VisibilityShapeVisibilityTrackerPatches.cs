using HarmonyLib;
using QSB.Patches;

namespace QSB.QuantumSync.Patches.Common.Visibility;

[HarmonyPatch(typeof(ShapeVisibilityTracker))]
internal class VisibilityShapeVisibilityTrackerPatches : QSBPatch
{
    public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShapeVisibilityTracker.IsVisibleUsingCameraFrustum))]
    public static bool IsVisibleUsingCameraFrustum(ShapeVisibilityTracker __instance, out bool __result)
    {
        __result = QuantumManager.IsVisibleUsingCameraFrustum(__instance, false).FoundPlayers;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShapeVisibilityTracker.IsVisible))]
    public static bool IsVisible(ShapeVisibilityTracker __instance, out bool __result)
    {
        __result = QuantumManager.IsVisible(__instance, false);
        return false;
    }
}
