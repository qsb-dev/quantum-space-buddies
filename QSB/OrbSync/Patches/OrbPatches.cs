using HarmonyLib;
using QSB.Messaging;
using QSB.OrbSync.Messages;
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
			var qsbOrb = __instance.GetWorldObject<QSBOrb>();
			qsbOrb.SendMessage(new OrbDragMessage(true));
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.CancelDrag))]
		public static bool CancelDrag(NomaiInterfaceOrb __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}
			if (!__instance._isBeingDragged)
			{
				return false;
			}
			var qsbOrb = __instance.GetWorldObject<QSBOrb>();
			if (!qsbOrb.TransformSync.HasAuthority)
			{
				return false;
			}
			qsbOrb.SendMessage(new OrbDragMessage(false));
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiInterfaceOrb.CheckSlotCollision))]
		public static bool CheckSlotCollision(NomaiInterfaceOrb __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}
			var qsbOrb = __instance.GetWorldObject<QSBOrb>();
			if (!qsbOrb.TransformSync.HasAuthority)
			{
				return false;
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
						qsbOrb.SendMessage(new OrbSlotMessage(slotIndex));
						break;
					}
				}
			}
			else if ((!__instance._occupiedSlot.IsAttractive() || __instance._isBeingDragged) && !__instance._occupiedSlot.CheckOrbCollision(__instance))
			{
				__instance._occupiedSlot = null;
				qsbOrb.SendMessage(new OrbSlotMessage(-1));
			}
			__instance._owCollider.SetActivation(__instance._occupiedSlot == null || !__instance._occupiedSlot.IsAttractive() || __instance._isBeingDragged);

			return false;
		}
	}
}
