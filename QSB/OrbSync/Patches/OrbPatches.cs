using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.OrbSync.Patches
{
	[HarmonyPatch]
	public class OrbPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(NomaiInterfaceOrb), nameof(NomaiInterfaceOrb.StartDragFromPosition))]
		public static void NomaiInterfaceOrb_StartDragFromPosition(bool __result, NomaiInterfaceOrb __instance)
		{
			if (__result)
			{
				var index = OrbManager.Orbs.IndexOf(__instance);
				if (index != -1)
				{
					QSBEventManager.FireEvent(EventNames.QSBOrbUser, index, true);
				}
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiInterfaceOrb), nameof(NomaiInterfaceOrb.CancelDrag))]
		public static void NomaiInterfaceOrb_CancelDrag(NomaiInterfaceOrb __instance)
		{
			var index = OrbManager.Orbs.IndexOf(__instance);
			if (index != -1 && __instance._isBeingDragged)
			{
				QSBEventManager.FireEvent(EventNames.QSBOrbUser, index, false);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiInterfaceSlot), nameof(NomaiInterfaceSlot.CheckOrbCollision))]
		public static bool NomaiInterfaceSlot_CheckOrbCollision(ref bool __result, NomaiInterfaceSlot __instance, NomaiInterfaceOrb orb,
			bool ____ignoreDraggedOrbs, float ____radius, float ____exitRadius, ref NomaiInterfaceOrb ____occupyingOrb)
		{
			if (____ignoreDraggedOrbs && orb.IsBeingDragged())
			{
				__result = false;
				return false;
			}

			var orbDistance = Vector3.Distance(orb.transform.position, __instance.transform.position);
			var triggerRadius = orb.IsBeingDragged() ? ____exitRadius : ____radius;
			if (____occupyingOrb == null && orbDistance < ____radius)
			{
				____occupyingOrb = orb;
				if (Time.timeSinceLevelLoad > 1f)
				{
					OrbManager.HandleSlotStateChange(__instance, orb, true);
					__instance.RaiseEvent("OnSlotActivated", __instance);
				}

				__result = true;
				return false;
			}

			if (____occupyingOrb == null || ____occupyingOrb != orb)
			{
				__result = false;
				return false;
			}

			if (orbDistance > triggerRadius)
			{
				OrbManager.HandleSlotStateChange(__instance, orb, false);
				____occupyingOrb = null;
				__instance.RaiseEvent("OnSlotDeactivated", __instance);
				__result = false;
				return false;
			}

			__result = true;
			return false;
		}
	}
}
