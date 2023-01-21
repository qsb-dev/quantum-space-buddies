using HarmonyLib;
using QSB.Patches;
using QSB.Utility;

namespace QSB.QuantumSync.Patches.Common.Visibility;

[HarmonyPatch(typeof(Shape))]
internal class VisibilityShapePatches : QSBPatch
{
    public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Shape.OnEnable))]
    public static void OnEnable(Shape __instance)
        => __instance.RaiseEvent(nameof(__instance.OnShapeActivated), __instance);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Shape.OnDisable))]
    public static void OnDisable(Shape __instance)
        => __instance.RaiseEvent(nameof(__instance.OnShapeDeactivated), __instance);
}
