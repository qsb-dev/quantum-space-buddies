using HarmonyLib;
using QSB.Patches;
using UnityEngine;

namespace QSB.QuantumSync.Patches.Common.Visibility;

[HarmonyPatch(typeof(ShapeVisibilityTracker))]
public class VisibilityShapeVisibilityTrackerPatches : QSBPatch
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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ShapeVisibilityTracker.IsInFrustum))]
    public static bool IsInFrustum(ShapeVisibilityTracker __instance, Plane[] frustumPlanes, out bool __result)
    {
	    foreach (var shape in __instance._shapes)
	    {
			// normally checks if enabled
			// helps prevent state change when owner leaves and we are observing
			// is this wrong? it feels wrong.
		    if (shape.IsVisible(frustumPlanes))
		    {
			    __result = true;
			    return false;
		    }
	    }

	    __result = false;
		return false;
    }
}
