using HarmonyLib;
using OWML.Common;
using QSB.ItemSync.Messages;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.Patches;

[HarmonyPatch(typeof(ItemTool))]
internal class ItemToolPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.MoveItemToCarrySocket))]
	public static void MoveItemToCarrySocket(OWItem item)
	{
		var qsbItem = item.GetWorldObject<IQSBItem>();
		QSBPlayerManager.LocalPlayer.HeldItem = qsbItem;
		qsbItem.SendMessage(new MoveToCarryMessage());
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.SocketItem))]
	public static void SocketItem(ItemTool __instance, OWItemSocket socket)
	{
		var item = __instance._heldItem;
		QSBPlayerManager.LocalPlayer.HeldItem = null;
		new SocketItemMessage(SocketMessageType.Socket, socket, item).Send();
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.StartUnsocketItem))]
	public static void StartUnsocketItem(OWItemSocket socket)
	{
		var item = socket.GetSocketedItem();
		var qsbItem = item.GetWorldObject<IQSBItem>();
		QSBPlayerManager.LocalPlayer.HeldItem = qsbItem;
		new SocketItemMessage(SocketMessageType.StartUnsocket, socket, item).Send();
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.CompleteUnsocketItem))]
	public static void CompleteUnsocketItem(ItemTool __instance)
	{
		var item = __instance._heldItem;
		new SocketItemMessage(SocketMessageType.CompleteUnsocket, null, item).Send();
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.DropItem))]
	public static bool DropItem(ItemTool __instance, RaycastHit hit, OWRigidbody targetRigidbody, IItemDropTarget customDropTarget)
	{
		Locator.GetPlayerAudioController().PlayDropItem(__instance._heldItem.GetItemType());
		var hitGameObject = hit.collider.gameObject;
		var gameObject2 = hitGameObject;
		var sectorGroup = gameObject2.GetComponent<ISectorGroup>();
		Sector sector = null;
		while (sectorGroup == null && gameObject2.transform.parent != null)
		{
			gameObject2 = gameObject2.transform.parent.gameObject;
			sectorGroup = gameObject2.GetComponent<ISectorGroup>();
		}

		if (sectorGroup != null)
		{
			sector = sectorGroup.GetSector();
			if (sector == null && sectorGroup is SectorCullGroup sectorCullGroup)
			{
				var controllingProxy = sectorCullGroup.GetControllingProxy();
				if (controllingProxy != null)
				{
					sector = controllingProxy.GetSector();
				}
			}
		}

		var parent = customDropTarget == null
			? targetRigidbody.transform
			: customDropTarget.GetItemDropTargetTransform(hit.collider.gameObject);
		var qsbItem = __instance._heldItem.GetWorldObject<IQSBItem>();
		__instance._heldItem.DropItem(hit.point, hit.normal, parent, sector, customDropTarget);
		__instance._heldItem = null;
		QSBPlayerManager.LocalPlayer.HeldItem = null;
		Locator.GetToolModeSwapper().UnequipTool();
		var parentSector = parent.GetComponentInChildren<Sector>();
		if (parentSector != null)
		{
			qsbItem.SendMessage(new DropItemMessage(hit.point, hit.normal, parentSector));
		}
		else
		{
			DebugLog.ToConsole($"Error - No sector found for rigidbody {targetRigidbody.name}!.", MessageType.Error);
		}

		return false;
	}
}
