using HarmonyLib;
using QSB.Events;
using QSB.OrbSync.WorldObjects;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.OrbSync.Patches
{
	[HarmonyPatch(typeof(NomaiInterfaceOrb))]
	public class OrbPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.StartDragFromPosition))]
		public static void StartDragFromPosition(NomaiInterfaceOrb __instance)
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
		[HarmonyPatch(nameof(NomaiInterfaceOrb.CancelDrag))]
		public static void CancelDrag(NomaiInterfaceOrb __instance)
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
			if (!qsbOrb.TransformSync.HasAuthority)
			{
				return;
			}
			QSBEventManager.FireEvent(EventNames.QSBOrbUser, qsbOrb, false);
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.CheckSlotCollision))]
		public static bool CheckSlotCollision(NomaiInterfaceOrb __instance,
			bool playAudio)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			if (!qsbOrb.TransformSync.HasAuthority)
			{
				return true;
			}

			if (__instance._occupiedSlot == null)
			{
				foreach (var slot in __instance._slots)
				{
					if (slot != null && slot.CheckOrbCollision(__instance))
					{
						__instance._occupiedSlot = slot;
						__instance._enterSlotTime = Time.time;
						if (slot.CancelsDragOnCollision())
						{
							__instance.CancelDrag();
						}
						if (playAudio && __instance._orbAudio != null && slot.GetPlayActivationAudio())
						{
							__instance._orbAudio.PlaySlotActivatedClip();
						}
						QSBEventManager.FireEvent(EventNames.QSBOrbSlot, qsbOrb, __instance._occupiedSlot, playAudio);
						break;
					}
				}
			}
			else if ((!__instance._occupiedSlot.IsAttractive() || __instance._isBeingDragged) && !__instance._occupiedSlot.CheckOrbCollision(__instance))
			{
				__instance._occupiedSlot = null;
				QSBEventManager.FireEvent(EventNames.QSBOrbSlot, qsbOrb, __instance._occupiedSlot, playAudio);
			}
			__instance._owCollider.SetActivation(__instance._occupiedSlot == null || !__instance._occupiedSlot.IsAttractive() || __instance._isBeingDragged);

			return false;
		}
	}
}
