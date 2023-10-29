using HarmonyLib;
using QSB.Patches;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
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
        // todo : cache this somewhere? seems slow.
	    var quantumObject = __instance.GetComponentInParent<QuantumObject>();

	    if (quantumObject == null)
	    {
            __result = false;
		    return true;
	    }

	    var worldObject = quantumObject.GetWorldObject<IQSBQuantumObject>();
	    foreach (var shape in __instance._shapes)
	    {
			// normally only checks if enabled and visible
			// helps prevent state change when owner leaves and we are observing
			// is this wrong? it feels wrong.
		    if ((shape.enabled || worldObject.ControllingPlayer == 0) && shape.IsVisible(frustumPlanes))
		    {
			    __result = true;
			    return false;
		    }
	    }

	    __result = false;
		return false;
    }
}
