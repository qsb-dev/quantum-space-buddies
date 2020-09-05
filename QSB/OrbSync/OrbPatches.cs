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
                GlobalMessenger<int>.FireEvent(EventNames.QSBOrbUser, WorldRegistry.OldOrbList.FindIndex(x => x == __instance));
            }
        }
    }
}
