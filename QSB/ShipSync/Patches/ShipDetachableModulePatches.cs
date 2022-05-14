using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.ShipSync.Messages;
using QSB.ShipSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ShipSync.Patches;

[HarmonyPatch(typeof(ShipDetachableModule))]
internal class ShipDetachableModulePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(ShipDetachableModule.Detach))]
	public static void Detach(ShipDetachableModule __instance)
	{
		__instance.GetWorldObject<QSBShipDetachableModule>().SendMessage(new ModuleDetachMessage());
	}
}
