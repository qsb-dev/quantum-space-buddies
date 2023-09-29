using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.ShipSync.Messages;
using QSB.ShipSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.ShipSync.Patches;

[HarmonyPatch(typeof(ShipDetachableModule))]
public class ShipDetachableModulePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ShipDetachableModule.Detach))]
	public static void Detach(ShipDetachableModule __instance)
	{
		if (Remote)
		{
			return;
		}

		if (__instance.isDetached)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBShipDetachableModule>().SendMessage(new ModuleDetachMessage());
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(ShipDetachableLeg.Detach))]
	public static void Detach(ShipDetachableLeg __instance)
	{
		if (Remote)
		{
			return;
		}

		if (__instance.isDetached)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBShipDetachableLeg>().SendMessage(new LegDetachMessage());
	}
}
