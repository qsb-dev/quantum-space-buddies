using HarmonyLib;
using QSB.Events;
using QSB.OrbSync.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.OrbSync.Patches
{
	[HarmonyPatch]
	public class OrbPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(NomaiInterfaceOrb), nameof(NomaiInterfaceOrb.StartDragFromPosition))]
		public static void NomaiInterfaceOrb_StartDragFromPosition(NomaiInterfaceOrb __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			if (!__instance._isBeingDragged)
			{
				return;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBOrbUser, qsbOrb, true);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiInterfaceOrb), nameof(NomaiInterfaceOrb.CancelDrag))]
		public static void NomaiInterfaceOrb_CancelDrag(NomaiInterfaceOrb __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			if (!__instance._isBeingDragged)
			{
				return;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBOrbUser, qsbOrb, false);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiInterfaceSlot), nameof(NomaiInterfaceSlot.CheckOrbCollision))]
		public static bool NomaiInterfaceSlot_CheckOrbCollision(NomaiInterfaceSlot __instance, ref bool __result,
			NomaiInterfaceOrb orb)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}
			var qsbOrbSlot = QSBWorldSync.GetWorldFromUnity<QSBOrbSlot>(__instance);
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(orb);
			if (!qsbOrb.TransformSync.HasAuthority)
			{
				return true;
			}

			if (__instance._ignoreDraggedOrbs && orb.IsBeingDragged())
			{
				__result = false;
				return false;
			}

			var orbDistance = Vector3.Distance(orb.transform.position, __instance.transform.position);
			var triggerRadius = orb.IsBeingDragged() ? __instance._exitRadius : __instance._radius;
			if (__instance._occupyingOrb == null && orbDistance < __instance._radius)
			{
				__instance._occupyingOrb = orb;
				if (Time.timeSinceLevelLoad > 1f)
				{
					__instance.RaiseEvent(nameof(__instance.OnSlotActivated), __instance);
					QSBEventManager.FireEvent(EventNames.QSBOrbSlot, qsbOrbSlot, qsbOrb, true);
				}

				__result = true;
				return false;
			}

			if (__instance._occupyingOrb == null || __instance._occupyingOrb != orb)
			{
				__result = false;
				return false;
			}

			if (orbDistance > triggerRadius)
			{
				__instance._occupyingOrb = null;
				__instance.RaiseEvent(nameof(__instance.OnSlotDeactivated), __instance);
				QSBEventManager.FireEvent(EventNames.QSBOrbSlot, qsbOrbSlot, qsbOrb, false);
				__result = false;
				return false;
			}

			__result = true;
			return false;
		}
	}
}
