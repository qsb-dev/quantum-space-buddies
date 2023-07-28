using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using System.Linq;
using UnityEngine;

namespace QSB.ItemSync.Patches;

public class ItemRemotePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	#region OWItem

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
	private static void base_PickUpItem(OWItem instance, Transform holdTranform) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.PickUpItem))]
	private static bool PickUpItem(DreamLanternItem __instance,
		Transform holdTranform)
	{
		if (!Remote)
		{
			return true;
		}

		base_PickUpItem(__instance, holdTranform);
		if (__instance._lanternType == DreamLanternType.Functioning)
		{
			// __instance.enabled = true;
		}

		if (__instance._lanternController != null)
		{
			__instance._lanternController.enabled = true;
			__instance._lanternController.SetDetectorScaleCompensation(__instance._lanternController.transform.lossyScale);
			__instance._lanternController.SetHeldByPlayer(true);
			// Locator.GetPlayerController().SetDreamLantern(__instance);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisionTorchItem), nameof(VisionTorchItem.PickUpItem))]
	private static bool PickUpItem(VisionTorchItem __instance,
		Transform holdTranform)
	{
		if (!Remote)
		{
			return true;
		}

		base_PickUpItem(__instance, holdTranform);
		if (__instance._visionBeam != null)
		{
			__instance._visionBeam.localScale = Vector3.one * 5f;
		}

		/*
		foreach (var renderer in __instance._worldModelRenderers)
		{
			renderer.SetActivation(false);
		}

		foreach (var renderer in __instance._viewModelRenderers)
		{
			renderer.SetActivation(true);
		}

		__instance.enabled = true;
		*/
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SimpleLanternItem), nameof(SimpleLanternItem.PickUpItem))]
	private static bool PickUpItem(SimpleLanternItem __instance,
		Transform holdTranform)
	{
		if (!Remote)
		{
			return true;
		}

		if (__instance._lit)
		{
			// Locator.GetFlashlight().TurnOff();
		}

		if (__instance._lightSourceVol != null)
		{
			__instance._lightSourceShape.radius = __instance._origLightSourceShapeRadius / holdTranform.localScale.x;
		}

		base_PickUpItem(__instance, holdTranform);
		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
	private static void base_DropItem(OWItem instance,
		Vector3 position,
		Vector3 normal,
		Transform parent,
		Sector sector,
		IItemDropTarget customDropTarget) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.DropItem))]
	private static bool DropItem(DreamLanternItem __instance,
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

		base_DropItem(__instance, position, normal, parent, sector, customDropTarget);
		// __instance.enabled = false;
		if (__instance._lanternController != null)
		{
			__instance._lanternController.SetDetectorScaleCompensation(__instance._lanternController.transform.lossyScale);
			__instance._lanternController.SetHeldByPlayer(false);
			__instance._lanternController.enabled = __instance._lanternController.IsLit();
			// Locator.GetPlayerController().SetDreamLantern(null);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisionTorchItem), nameof(VisionTorchItem.DropItem))]
	private static bool DropItem(VisionTorchItem __instance,
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

		if (__instance._isProjecting)
		{
			__instance._mindProjectorTrigger.SetProjectorActive(false);
			__instance._isProjecting = false;
		}

		// if (Locator.GetDreamWorldController().IsInDream())
		// {
		base_DropItem(__instance, position, normal, parent, sector, customDropTarget);
		// }

		if (__instance._visionBeam != null)
		{
			__instance._visionBeam.localScale = Vector3.one;
		}

		/*
		foreach (var renderer in __instance._worldModelRenderers)
		{
			renderer.SetActivation(true);
		}

		foreach (var renderer in __instance._viewModelRenderers)
		{
			renderer.SetActivation(false);
		}

		__instance.enabled = false;
		*/
		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItem), nameof(OWItem.SocketItem))]
	private static void base_SocketItem(OWItem instance, Transform socketTransform, Sector sector) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternItem), nameof(DreamLanternItem.SocketItem))]
	private static bool SocketItem(DreamLanternItem __instance,
		Transform socketTransform, Sector sector)
	{
		if (!Remote)
		{
			return true;
		}

		base_SocketItem(__instance, socketTransform, sector);
		// __instance.enabled = false;
		if (__instance._lanternController != null)
		{
			__instance._lanternController.SetDetectorScaleCompensation(__instance._lanternController.transform.lossyScale);
			__instance._lanternController.SetSocketed(true);
			__instance._lanternController.SetHeldByPlayer(false);
			__instance._lanternController.enabled = __instance._lanternController.IsLit();
			// Locator.GetPlayerController().SetDreamLantern(null);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisionTorchItem), nameof(VisionTorchItem.SocketItem))]
	private static bool SocketItem(VisionTorchItem __instance,
		Transform socketTransform, Sector sector)
	{
		if (!Remote)
		{
			return true;
		}

		if (__instance._isProjecting)
		{
			__instance._mindProjectorTrigger.SetProjectorActive(false);
			__instance._isProjecting = false;
		}

		base_SocketItem(__instance, socketTransform, sector);
		if (__instance._visionBeam != null)
		{
			__instance._visionBeam.localScale = Vector3.one;
		}

		/*
		foreach (var renderer in __instance._worldModelRenderers)
		{
			renderer.SetActivation(true);
		}

		foreach (var renderer in __instance._viewModelRenderers)
		{
			renderer.SetActivation(false);
		}

		__instance.enabled = false;
		*/
		return false;
	}

	#endregion

	#region OWItemSocket

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.PlaceIntoSocket))]
	private static bool base_PlaceIntoSocket(OWItemSocket instance, OWItem item) => default;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternSocket), nameof(DreamLanternSocket.PlaceIntoSocket))]
	private static bool PlaceIntoSocket(DreamLanternSocket __instance, ref bool __result,
		OWItem item)
	{
		if (!Remote)
		{
			return true;
		}

		if (base_PlaceIntoSocket(__instance, item))
		{
			// Locator.GetDreamWorldController().SetPlayerLanternSocket(__instance);
			__result = true;
			return false;
		}

		__result = false;
		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.RemoveFromSocket))]
	private static OWItem base_RemoveFromSocket(OWItemSocket instance) => default;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamLanternSocket), nameof(DreamLanternSocket.RemoveFromSocket))]
	private static bool RemoveFromSocket(DreamLanternSocket __instance, ref OWItem __result)
	{
		if (!Remote)
		{
			return true;
		}

		var owitem = base_RemoveFromSocket(__instance);
		if (owitem != null)
		{
			// Locator.GetDreamWorldController().SetPlayerLanternSocket(null);
		}

		__result = owitem;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SlideReelSocket), nameof(SlideReelSocket.RemoveFromSocket))]
	private static bool RemoveFromSocket(SlideReelSocket __instance, ref OWItem __result)
	{
		if (!Remote)
		{
			return true;
		}

		var socketedItem = (SlideReelItem)__instance._socketedItem;
		var player = QSBPlayerManager.PlayerList.First(x => x.HeldItem?.AttachedObject == socketedItem);
		socketedItem.SetSocketLocalDir(__instance.CalcCorrectUnsocketDir(player.Camera.transform));
		__result = base_RemoveFromSocket(__instance);
		return false;
	}

	#endregion
}
