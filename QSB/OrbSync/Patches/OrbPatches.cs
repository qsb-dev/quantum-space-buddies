using HarmonyLib;
using QSB.Events;
using QSB.OrbSync.WorldObjects;
using QSB.Patches;
using QSB.Utility;
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
			if (!__instance._isBeingDragged)
			{
				return;
			}
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBOrbDrag, qsbOrb, true);
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.CancelDrag))]
		public static void CancelDrag(NomaiInterfaceOrb __instance)
		{
			if (!__instance._isBeingDragged)
			{
				return;
			}
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			if (!qsbOrb.TransformSync.HasAuthority)
			{
				return;
			}
			QSBEventManager.FireEvent(EventNames.QSBOrbDrag, qsbOrb, false);
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.MoveTowardPosition))]
		public static bool MoveTowardPosition(NomaiInterfaceOrb __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}
			var qsbOrb = QSBWorldSync.GetWorldFromUnity<QSBOrb>(__instance);
			if (qsbOrb.TransformSync.HasAuthority)
			{
				return true;
			}

			var pointVelocity = __instance._parentBody.GetPointVelocity(__instance._orbBody.GetPosition());
			__instance._orbBody.SetVelocity(pointVelocity);
			if (!__instance._applyForcesWhileMoving)
			{
				__instance._forceApplier.SetApplyForces(false);
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.CheckSlotCollision))]
		public static bool CheckSlotCollision(NomaiInterfaceOrb __instance)
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
				for (var slotIndex = 0; slotIndex < __instance._slots.Length; slotIndex++)
				{
					var slot = __instance._slots[slotIndex];
					if (slot != null && slot.CheckOrbCollision(__instance))
					{
						__instance._occupiedSlot = slot;
						__instance._enterSlotTime = Time.time;
						if (slot.CancelsDragOnCollision())
						{
							__instance.CancelDrag();
						}
						if (__instance._orbAudio != null && slot.GetPlayActivationAudio())
						{
							__instance._orbAudio.PlaySlotActivatedClip();
						}
						QSBEventManager.FireEvent(EventNames.QSBOrbSlot, qsbOrb, slotIndex);
						break;
					}
				}
			}
			else if ((!__instance._occupiedSlot.IsAttractive() || __instance._isBeingDragged) && !__instance._occupiedSlot.CheckOrbCollision(__instance))
			{
				__instance._occupiedSlot = null;
				QSBEventManager.FireEvent(EventNames.QSBOrbSlot, qsbOrb, -1);
			}
			__instance._owCollider.SetActivation(__instance._occupiedSlot == null || !__instance._occupiedSlot.IsAttractive() || __instance._isBeingDragged);

			return false;
		}
	}
}
