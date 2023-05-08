using HarmonyLib;
using QSB.Messaging;
using QSB.OrbSync.Messages;
using QSB.OrbSync.WorldObjects;
using QSB.OwnershipSync;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.OrbSync.Patches;

[HarmonyPatch(typeof(NomaiInterfaceOrb))]
public class OrbPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(NomaiInterfaceOrb.StartDragFromPosition))]
	private static bool StartDragFromPosition(NomaiInterfaceOrb __instance, ref bool __result,
		Vector3 manipPos)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (__instance._orbBody.IsSuspended() || __instance._isBeingDragged)
		{
			__result = false;
			return false;
		}

		if (__instance.RecentlyEnteredSlot())
		{
			__instance._loseFocusToStartDrag = true;
		}

		if (Vector3.Distance(manipPos, __instance.transform.position) < __instance._startDragDist)
		{
			if (!__instance._loseFocusToStartDrag)
			{
				__instance._isBeingDragged = true;
				__instance._interactibleCollider.enabled = false;
				if (__instance._orbAudio != null)
				{
					__instance._orbAudio.PlayStartDragClip();
				}

				var qsbOrb = __instance.GetWorldObject<QSBOrb>();
				qsbOrb.SendMessage(new OrbDragMessage(true));
				qsbOrb.NetworkBehaviour.netIdentity.UpdateOwnerQueue(OwnerQueueAction.Force);
			}
		}
		else
		{
			__instance._loseFocusToStartDrag = false;
		}

		__result = __instance._isBeingDragged;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(NomaiInterfaceOrb.CancelDrag))]
	private static bool CancelDrag(NomaiInterfaceOrb __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (!__instance._isBeingDragged)
		{
			return false;
		}

		var qsbOrb = __instance.GetWorldObject<QSBOrb>();
		if (!qsbOrb.NetworkBehaviour.isOwned)
		{
			return false;
		}

		qsbOrb.SendMessage(new OrbDragMessage(false));
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(NomaiInterfaceOrb.CheckSlotCollision))]
	private static bool CheckSlotCollision(NomaiInterfaceOrb __instance,
		bool playAudio)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbOrb = __instance.GetWorldObject<QSBOrb>();
		if (qsbOrb.NetworkBehaviour.isOwned)
		{
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

						if (playAudio && __instance._orbAudio != null && slot.GetPlayActivationAudio())
						{
							__instance._orbAudio.PlaySlotActivatedClip();
						}

						qsbOrb.SendMessage(new OrbSlotMessage(slotIndex, playAudio));
						break;
					}
				}
			}
			else if ((!__instance._occupiedSlot.IsAttractive() || __instance._isBeingDragged) && !__instance._occupiedSlot.CheckOrbCollision(__instance))
			{
				__instance._occupiedSlot = null;
				qsbOrb.SendMessage(new OrbSlotMessage(-1, playAudio));
			}
		}

		__instance._owCollider.SetActivation(__instance._occupiedSlot == null || !__instance._occupiedSlot.IsAttractive() || __instance._isBeingDragged);

		return false;
	}
}