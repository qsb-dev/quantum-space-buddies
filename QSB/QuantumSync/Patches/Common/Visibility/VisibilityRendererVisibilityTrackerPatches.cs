using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using System.Linq;
using UnityEngine;

namespace QSB.QuantumSync.Patches.Common.Visibility;

[HarmonyPatch(typeof(RendererVisibilityTracker))]
internal class VisibilityRendererVisibilityTrackerPatches : QSBPatch
{
    public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RendererVisibilityTracker), nameof(RendererVisibilityTracker.IsVisibleUsingCameraFrustum))]
    public static bool RendererVisibilityTracker_IsVisibleUsingCameraFrustum(RendererVisibilityTracker __instance, out bool __result)
    {
        __result = QSBPlayerManager.GetPlayersWithCameras()
                       .Any(x => GeometryUtility.TestPlanesAABB(x.Camera.GetFrustumPlanes(), __instance._renderer.bounds))
                   && (!__instance._checkFrustumOcclusion || QSBPlayerManager.GetPlayersWithCameras()
                       .Any(x => !__instance.IsOccludedFromPosition(x.Camera.transform.position)));
        return false;
    }
}
