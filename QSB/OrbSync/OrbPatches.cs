using QSB.Events;
using QSB.WorldSync;
using UnityEngine;

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

        public static bool CheckOrbCollision(ref bool __result, NomaiInterfaceSlot __instance, NomaiInterfaceOrb orb, 
            bool ____ignoreDraggedOrbs, float ____radius, float ____exitRadius, ref NomaiInterfaceOrb ____occupyingOrb)
        {
            if (____ignoreDraggedOrbs && orb.IsBeingDragged())
            {
                __result = false;
                return false;
            }
            var orbDistance = Vector3.Distance(orb.transform.position, __instance.transform.position);
            var triggerRadius = !orb.IsBeingDragged() ? ____radius : ____exitRadius;
            if (____occupyingOrb == null && orbDistance < ____radius)
            {
                ____occupyingOrb = orb;
                if (Time.timeSinceLevelLoad > 1f)
                {
                    WorldRegistry.HandleSlotStateChange(__instance, orb, true);
                    WorldRegistry.RaiseEvent(__instance, "OnSlotActivated");
                }
                __result = true;
                return false;
            }
            if (!(____occupyingOrb != null) || !(____occupyingOrb == orb))
            {
                __result = false;
                return false;
            }
            if (orbDistance > triggerRadius)
            {
                WorldRegistry.HandleSlotStateChange(__instance, orb, false);
                ____occupyingOrb = null;
                WorldRegistry.RaiseEvent(__instance, "OnSlotDeactivated");
                __result = false;
                return false;
            }
            __result = true;
            return false;
        }

        public static void AddPatches()
        {
            QSB.Helper.HarmonyHelper.AddPostfix<NomaiInterfaceOrb>("StartDragFromPosition", typeof(OrbPatches), nameof(StartDragCallEvent));
            QSB.Helper.HarmonyHelper.AddPrefix<NomaiInterfaceSlot>("CheckOrbCollision", typeof(OrbPatches), nameof(CheckOrbCollision));
        }
    }
}
