using HarmonyLib;
using QSB.Patches;

namespace QSB.AnglerFish.Patches {
    [HarmonyPatch]
    internal class AnglerPatches : QSBPatch {
        public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;
    }
}
