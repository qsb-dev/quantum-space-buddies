using HarmonyLib;
using OWML.Common;
using QSB.Events;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ItemSync.Patches
{
	[HarmonyPatch]
	internal class ItemPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.MoveItemToCarrySocket))]
		public static bool ItemTool_MoveItemToCarrySocket(OWItem item)
		{
			var qsbObj = (IQSBOWItem)QSBWorldSync.GetWorldFromUnity(item);
			var itemId = QSBWorldSync.GetIdFromTypeSubset(qsbObj);
			QSBPlayerManager.LocalPlayer.HeldItem = qsbObj;
			QSBEventManager.FireEvent(EventNames.QSBMoveToCarry, itemId);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.SocketItem))]
		public static bool ItemTool_SocketItem(OWItem ____heldItem, OWItemSocket socket)
		{
			var qsbObj = (IQSBOWItem)QSBWorldSync.GetWorldFromUnity(____heldItem);
			var socketId = QSBWorldSync.GetIdFromTypeSubset((IQSBOWItemSocket)QSBWorldSync.GetWorldFromUnity(socket));
			var itemId = QSBWorldSync.GetIdFromTypeSubset(qsbObj);
			QSBPlayerManager.LocalPlayer.HeldItem = null;
			QSBEventManager.FireEvent(EventNames.QSBSocketItem, socketId, itemId, SocketEventType.Socket);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.StartUnsocketItem))]
		public static bool ItemTool_StartUnsocketItem(OWItemSocket socket)
		{
			var item = (IQSBOWItem)QSBWorldSync.GetWorldFromUnity(socket.GetSocketedItem());
			QSBPlayerManager.LocalPlayer.HeldItem = item;
			var socketId = QSBWorldSync.GetIdFromTypeSubset((IQSBOWItemSocket)QSBWorldSync.GetWorldFromUnity(socket));
			QSBEventManager.FireEvent(EventNames.QSBSocketItem, socketId, 0, SocketEventType.StartUnsocket);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.CompleteUnsocketItem))]
		public static bool ItemTool_CompleteUnsocketItem(OWItem ____heldItem)
		{
			var itemId = QSBWorldSync.GetIdFromTypeSubset((IQSBOWItem)QSBWorldSync.GetWorldFromUnity(____heldItem));
			QSBEventManager.FireEvent(EventNames.QSBSocketItem, 0, itemId, SocketEventType.CompleteUnsocket);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ItemTool), nameof(ItemTool.DropItem))]
		public static bool ItemTool_DropItem(RaycastHit hit, OWRigidbody targetRigidbody, IItemDropTarget customDropTarget, ref OWItem ____heldItem)
		{
			Locator.GetPlayerAudioController().PlayDropItem(____heldItem.GetItemType());
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
				if (sector == null && sectorGroup is SectorCullGroup)
				{
					var controllingProxy = (sectorGroup as SectorCullGroup).GetControllingProxy();
					if (controllingProxy != null)
					{
						sector = controllingProxy.GetSector();
					}
				}
			}

			var parent = (customDropTarget == null)
				? targetRigidbody.transform
				: customDropTarget.GetItemDropTargetTransform(hit.collider.gameObject);
			var objectId = QSBWorldSync.GetIdFromTypeSubset((IQSBOWItem)QSBWorldSync.GetWorldFromUnity(____heldItem));
			____heldItem.DropItem(hit.point, hit.normal, parent, sector, customDropTarget);
			____heldItem = null;
			QSBPlayerManager.LocalPlayer.HeldItem = null;
			Locator.GetToolModeSwapper().UnequipTool();
			var parentSector = parent.GetComponentInChildren<Sector>();
			if (parentSector != null)
			{
				var localPos = parentSector.transform.InverseTransformPoint(hit.point);
				QSBEventManager.FireEvent(EventNames.QSBDropItem, objectId, localPos, hit.normal, parentSector);
				return false;
			}

			DebugLog.ToConsole($"Error - No sector found for rigidbody {targetRigidbody.name}!.", MessageType.Error);
			return false;
		}
	}
}
