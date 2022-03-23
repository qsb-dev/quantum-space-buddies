using HarmonyLib;
using QSB.Patches;

namespace QSB.ItemSync.Patches;

internal class ItemRemotePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(OWItem), nameof(OWItem.PickUpItem))]
	private static bool OWItem_PickUpItem(OWItem __instance)
	{
		return false;
	}

	[HarmonyPatch(typeof(OWItem), nameof(OWItem.DropItem))]
	private static bool OWItem_DropItem(OWItem __instance)
	{
		return false;
	}

	[HarmonyPatch(typeof(OWItem), nameof(OWItem.SocketItem))]
	private static bool OWItem_SocketItem(OWItem __instance)
	{
		return false;
	}

	[HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.PlaceIntoSocket))]
	private static bool OWItemSocket_PlaceIntoSocket(OWItem __instance)
	{
		return false;
	}

	[HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.RemoveFromSocket))]
	private static bool OWItemSocket_RemoveFromSocket(OWItem __instance)
	{
		return false;
	}
}
