namespace QSB.OrbSync
{
    public static class OrbSlotPatches
    {
        public static bool CheckOrbCollisionSkip(NomaiInterfaceOrb orb)
        {
            return false;
        }

        /*
        public static void StartDragCallEvent(bool __result, NomaiInterfaceOrb __instance)
        {
            if (__result)
            {
                DebugLog.DebugWrite("Sending using message for " + WorldRegistry.OldOrbList.FindIndex(x => x == __instance));
                GlobalMessenger<int>.FireEvent(EventNames.QSBOrbUser, WorldRegistry.OldOrbList.FindIndex(x => x == __instance));
            }
            GlobalMessenger<int, bool>.FireEvent(EventNames.QSBOrbStatus, WorldRegistry.OldOrbList.FindIndex(x => x == __instance), __result);
        }

        public static void CancelDrag(NomaiInterfaceOrb __instance)
        {
            GlobalMessenger<int, bool>.FireEvent(EventNames.QSBOrbStatus, WorldRegistry.OldOrbList.FindIndex(x => x == __instance), false);
        }

        public static void SetPosition(NomaiInterfaceOrb __instance)
        {
            GlobalMessenger<int, bool>.FireEvent(EventNames.QSBOrbStatus, WorldRegistry.OldOrbList.FindIndex(x => x == __instance), false);
        }

        public static bool SetOrbPosition(NomaiInterfaceOrb __instance)
        {
            return WorldRegistry.IsOrbControlledLocally(__instance);
        }
        */
    }
}
