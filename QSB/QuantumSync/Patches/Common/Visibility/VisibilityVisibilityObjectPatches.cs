using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using System.Linq;

namespace QSB.QuantumSync.Patches.Common.Visibility;

[HarmonyPatch(typeof(VisibilityObject))]
public class VisibilityVisibilityObjectPatches : QSBPatch
{
    public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(VisibilityObject.CheckIllumination))]
    public static bool CheckIllumination(VisibilityObject __instance, out bool __result)
    {
        if (!__instance._checkIllumination)
        {
            __result = true;
            return false;
        }

        var point = __instance.transform.TransformPoint(__instance._localIlluminationOffset);
        var (localFlashlight, playerFlashlights) = QSBPlayerManager.GetPlayerFlashlights();

        if (localFlashlight.CheckIlluminationAtPoint(point, __instance._illuminationRadius))
        {
            __result = true;
            return false;
        }

        if (playerFlashlights.Any(x => x.CheckIlluminationAtPoint(point, __instance._illuminationRadius)))
        {
            __result = true;
            return false;
        }

        var (localProbe, playerProbes) = QSBPlayerManager.GetPlayerProbes();

        if (localProbe != null
            && localProbe.IsLaunched()
            && localProbe.CheckIlluminationAtPoint(point, __instance._illuminationRadius))
        {
            __result = true;
            return false;
        }

        if (playerProbes.Any(x => x.IsLaunched() && x.CheckIlluminationAtPoint(point, __instance._illuminationRadius)))
        {
            __result = true;
            return false;
        }

        if (QSBPlayerManager.GetThrusterLightTrackers()
            .Any(x => x.CheckIlluminationAtPoint(point, __instance._illuminationRadius)))
        {
            __result = true;
            return false;
        }

        if (__instance._lightSources != null)
        {
            foreach (var light in __instance._lightSources)
            {
                if (light.intensity > 0f && light.range > 0f)
                {
                    __result = true;
                    return false;
                }
            }
        }

        __result = false;
        return false;
    }
}
