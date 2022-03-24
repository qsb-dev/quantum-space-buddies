using HarmonyLib;
using QSB.Patches;
using UnityEngine;

namespace QSB.ItemSync.Patches;

internal class ItemRemotePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	#region item

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
	private static void OWItem_PickUpItem(OWItem instance, Transform holdTranform) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.PickUpItem))]
	private static bool DreamLanternItem_PickUpItem(DreamLanternItem __instance,
		Transform holdTranform)
	{
		if (!Remote)
		{
			return true;
		}

		OWItem_PickUpItem(__instance, holdTranform);
		if (__instance._lanternType == DreamLanternType.Functioning)
		{
			__instance.enabled = true;
		}

		if (__instance._lanternController != null)
		{
			__instance._lanternController.enabled = true;
			__instance._lanternController.SetDetectorScaleCompensation(__instance._lanternController.transform.lossyScale);
			__instance._lanternController.SetHeldByPlayer(true);
		}

		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
	private static void OWItem_DropItem(OWItem instance,
		Vector3 position,
		Vector3 normal,
		Transform parent,
		Sector sector,
		IItemDropTarget customDropTarget) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.DropItem))]
	private static bool DreamLanternItem_DropItem(DreamLanternItem __instance,
		Vector3 position,
		Vector3 normal,
		Transform parent,
		Sector sector,
		IItemDropTarget customDropTarget)
	{
		if (!Remote)
		{
			return true;
		}

		OWItem_DropItem(__instance, position, normal, parent, sector, customDropTarget);
		__instance.enabled = false;
		if (__instance._lanternController != null)
		{
			__instance._lanternController.SetDetectorScaleCompensation(__instance._lanternController.transform.lossyScale);
			__instance._lanternController.SetHeldByPlayer(false);
			__instance._lanternController.enabled = __instance._lanternController.IsLit();
		}

		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItem), nameof(OWItem.SocketItem))]
	private static void OWItem_SocketItem(OWItem instance, Transform socketTransform, Sector sector) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.SocketItem))]
	private static bool DreamLanternItem_SocketItem(DreamLanternItem __instance,
		Transform socketTransform, Sector sector)
	{
		if (!Remote)
		{
			return true;
		}

		__instance.SocketItem(socketTransform, sector);
		__instance.enabled = false;
		if (__instance._lanternController != null)
		{
			__instance._lanternController.SetDetectorScaleCompensation(__instance._lanternController.transform.lossyScale);
			__instance._lanternController.SetSocketed(true);
			__instance._lanternController.SetHeldByPlayer(false);
			__instance._lanternController.enabled = __instance._lanternController.IsLit();
		}

		return false;
	}

	#endregion

	#region item socket

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.PlaceIntoSocket))]
	private static bool OWItemSocket_PlaceIntoSocket(OWItemSocket instance, OWItem item) => default;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternSocket), nameof(DreamLanternSocket.PlaceIntoSocket))]
	private static bool DreamLanternSocket_PlaceIntoSocket(DreamLanternSocket __instance, ref bool __result,
		OWItem item)
	{
		if (!Remote)
		{
			return true;
		}

		if (OWItemSocket_PlaceIntoSocket(__instance, item))
		{
			__result = true;
			return false;
		}

		__result = false;
		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.RemoveFromSocket))]
	private static OWItem OWItemSocket_RemoveFromSocket(OWItemSocket instance) => default;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternSocket), nameof(DreamLanternSocket.RemoveFromSocket))]
	private static bool DreamLanternSocket_RemoveFromSocket(DreamLanternSocket __instance, ref OWItem __result)
	{
		if (!Remote)
		{
			return true;
		}

		var owitem = OWItemSocket_RemoveFromSocket(__instance);
		if (owitem != null) { }

		__result = owitem;
		return false;
	}

	#endregion
}
