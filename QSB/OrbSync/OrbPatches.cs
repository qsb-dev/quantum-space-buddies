using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.OrbSync
{
    public static class OrbPatches
    {
        public static void StartDragCallEvent(bool __result, NomaiInterfaceOrb __instance)
        {
            if (__result)
            {
                DebugLog.DebugWrite("Sending using message for " + WorldRegistry.OldOrbList.FindIndex(x => x == __instance));
                GlobalMessenger<int>.FireEvent(EventNames.QSBOrbUser, WorldRegistry.OldOrbList.FindIndex(x => x == __instance));
            }
        }
    }
}
