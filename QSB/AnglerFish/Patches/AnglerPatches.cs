using HarmonyLib;
using QSB.AnglerFish.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.AnglerFish.Patches {
    [HarmonyPatch]
    internal class AnglerPatches : QSBPatch {
        public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnSectorOccupantsUpdated))]
        private static void SectorUpdated(AnglerfishController __instance) =>
            QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance).HandleEvent();
    }
}
