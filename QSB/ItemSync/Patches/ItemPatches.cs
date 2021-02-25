using QSB.Events;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System.Reflection;
using UnityEngine;

namespace QSB.ItemSync.Patches
{
	internal class ItemPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<ItemTool>("SocketItem", typeof(ItemPatches), nameof(ItemTool_SocketItem));
			QSBCore.Helper.HarmonyHelper.AddPrefix<ItemTool>("StartUnsocketItem", typeof(ItemPatches), nameof(ItemTool_StartUnsocketItem));
			QSBCore.Helper.HarmonyHelper.AddPrefix<ItemTool>("DropItem", typeof(ItemPatches), nameof(ItemTool_DropItem));
		}

		public override void DoUnpatches()
		{

		}

		public static bool ItemTool_SocketItem(OWItem ____heldItem, OWItemSocket socket)
		{
			DebugLog.DebugWrite($"Socket item {____heldItem.name} into socket {socket.name}.");
			var objectId = QSBWorldSync.GetIdFromTypeSubset(ItemManager.GetObject(socket));
			return true;
		}

		public static bool ItemTool_StartUnsocketItem(ItemTool __instance, ref OWItem ____heldItem, ref bool ____waitForUnsocketAnimation, OWItemSocket socket)
		{
			var owitem = socket.RemoveFromSocket();
			____heldItem = owitem;
			DebugLog.DebugWrite($"Start unsocket item {____heldItem.name} from socket {socket.name}.");
			if (____heldItem.IsAnimationPlaying())
			{
				____waitForUnsocketAnimation = true;
			}
			else
			{
				__instance.GetType().GetMethod("CompleteUnsocketItem", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
			}
			return false;
		}

		public static bool ItemTool_DropItem(RaycastHit hit, OWRigidbody targetRigidbody, DetachableFragment detachableFragment, ref OWItem ____heldItem)
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
			}
			var parent = (detachableFragment != null)
				? detachableFragment.transform
				: targetRigidbody.transform;
			var objectId = QSBWorldSync.GetIdFromTypeSubset(ItemManager.GetObject(____heldItem));
			____heldItem.DropItem(hit.point, hit.normal, parent, sector, detachableFragment);
			____heldItem = null;
			Locator.GetToolModeSwapper().UnequipTool();
			var parentSector = parent.GetComponentInChildren<Sector>();
			if (parentSector != null)
			{
				var localPos = parentSector.transform.InverseTransformPoint(hit.point);
				QSBEventManager.FireEvent(EventNames.QSBDropItem, objectId, localPos, hit.normal, parentSector);
				return false;
			}
			var localPosition = sector.transform.InverseTransformPoint(hit.point);
			QSBEventManager.FireEvent(EventNames.QSBDropItem, objectId, localPosition, hit.normal, sector);
			return false;
		}
	}
}
