using HarmonyLib;
using QSB.ItemSync.Messages;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
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
		qsbItem.ItemState.HasBeenInteractedWith = true;
		qsbItem.ItemState.State = ItemStateType.Held;
		qsbItem.ItemState.HoldingPlayer = QSBPlayerManager.LocalPlayer;
		qsbItem.SendMessage(new MoveToCarryMessage(QSBPlayerManager.LocalPlayer.PlayerId));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.SocketItem))]
	public static void SocketItem(ItemTool __instance, OWItemSocket socket)
	{
		var item = __instance._heldItem;
		QSBPlayerManager.LocalPlayer.HeldItem = null;
		var qsbItem = item.GetWorldObject<IQSBItem>();
		qsbItem.ItemState.State = ItemStateType.Socketed;
		qsbItem.ItemState.Socket = socket;
		qsbItem.SendMessage(new SocketItemMessage(SocketMessageType.Socket, socket));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.StartUnsocketItem))]
	public static void StartUnsocketItem(OWItemSocket socket)
	{
		var item = socket.GetSocketedItem();
		var qsbItem = item.GetWorldObject<IQSBItem>();
		qsbItem.ItemState.HasBeenInteractedWith = true;
		QSBPlayerManager.LocalPlayer.HeldItem = qsbItem;
		qsbItem.SendMessage(new SocketItemMessage(SocketMessageType.StartUnsocket, socket));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.CompleteUnsocketItem))]
	public static void CompleteUnsocketItem(ItemTool __instance)
	{
		var item = __instance._heldItem;
		var qsbItem = item.GetWorldObject<IQSBItem>();
		qsbItem.SendMessage(new SocketItemMessage(SocketMessageType.CompleteUnsocket, null));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ItemTool.DropItem))]
	public static bool DropItem(ItemTool __instance, RaycastHit hit, OWRigidbody targetRigidbody, IItemDropTarget customDropTarget)
	{
		Locator.GetPlayerAudioController().PlayDropItem(__instance._heldItem.GetItemType());
		var gameObject = hit.collider.gameObject;
		var component = gameObject.GetComponent<ISectorGroup>();
		Sector sector = null;

		while (component == null && gameObject.transform.parent != null)
		{
			gameObject = gameObject.transform.parent.gameObject;
			component = gameObject.GetComponent<ISectorGroup>();
		}

		if (component != null)
		{
			sector = component.GetSector();
			if (sector == null && component is SectorCullGroup sectorCullGroup)
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
		customDropTarget?.AddDroppedItem(hit.collider.gameObject, __instance._heldItem);

		__instance._heldItem = null;
		QSBPlayerManager.LocalPlayer.HeldItem = null;

		Locator.GetToolModeSwapper().UnequipTool();

		qsbItem.SendMessage(new DropItemMessage(hit.point, hit.normal, parent, sector, customDropTarget, targetRigidbody));

		qsbItem.ItemState.State = ItemStateType.OnGround;
		qsbItem.ItemState.Parent = parent;
		qsbItem.ItemState.LocalPosition = parent.InverseTransformPoint(hit.point);
		qsbItem.ItemState.LocalNormal = parent.InverseTransformDirection(hit.normal);
		qsbItem.ItemState.Sector = sector;
		qsbItem.ItemState.CustomDropTarget = customDropTarget;
		qsbItem.ItemState.Rigidbody = targetRigidbody;

		return false;
	}
}
