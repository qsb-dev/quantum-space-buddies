using HarmonyLib;
using OWML.Common;
using QSB.ItemSync.Messages;
using QSB.ItemSync.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.Patches;

[HarmonyPatch]
internal class ItemPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.MoveItemToCarrySocket))]
	public static bool ItemTool_MoveItemToCarrySocket(OWItem item)
	{
		var qsbObj = item.GetWorldObject<IQSBItem>();
		QSBPlayerManager.LocalPlayer.HeldItem = qsbObj;
		qsbObj.SendMessage(new MoveToCarryMessage());
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.SocketItem))]
	public static bool ItemTool_SocketItem(ItemTool __instance, OWItemSocket socket)
	{
		var qsbObj = __instance._heldItem.GetWorldObject<IQSBItem>();
		var socketId = socket.GetWorldObject<QSBItemSocket>().ObjectId;
		var itemId = qsbObj.ObjectId;
		QSBPlayerManager.LocalPlayer.HeldItem = null;
		new SocketItemMessage(SocketMessageType.Socket, socketId, itemId).Send();
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.StartUnsocketItem))]
	public static bool ItemTool_StartUnsocketItem(OWItemSocket socket)
	{
		var item = socket.GetSocketedItem().GetWorldObject<IQSBItem>();
		QSBPlayerManager.LocalPlayer.HeldItem = item;
		var socketId = socket.GetWorldObject<QSBItemSocket>().ObjectId;
		new SocketItemMessage(SocketMessageType.StartUnsocket, socketId).Send();
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.CompleteUnsocketItem))]
	public static bool ItemTool_CompleteUnsocketItem(ItemTool __instance)
	{
		var itemId = __instance._heldItem.GetWorldObject<IQSBItem>().ObjectId;
		new SocketItemMessage(SocketMessageType.CompleteUnsocket, itemId: itemId).Send();
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.DropItem))]
	public static bool ItemTool_DropItem(ItemTool __instance, RaycastHit hit, OWRigidbody targetRigidbody, IItemDropTarget customDropTarget)
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

		var parent = (customDropTarget == null)
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