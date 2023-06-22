using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.QuantumSync.Messages;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(QuantumObject))]
public class QuantumObjectPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumObject.IsLockedByPlayerContact))]
	public static bool IsLockedByPlayerContact(out bool __result, QuantumObject __instance)
	{
		var playersEntangled = QuantumManager.GetEntangledPlayers(__instance);
		__result = playersEntangled.Count() != 0 && __instance.IsIlluminated();
		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(QuantumObject.SetIsQuantum))]
	public static void SetIsQuantum(QuantumObject __instance)
	{
		if (QSBWorldSync.AllObjectsReady)
		{
			__instance.GetWorldObject<IQSBQuantumObject>().SendMessage(new SetIsQuantumMessage(__instance.IsQuantum()));
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumObject.OnProbeSnapshot))]
	public static bool OnProbeSnapshot()
	{
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumObject.OnProbeSnapshotRemoved))]
	public static bool OnProbeSnapshotRemoved()
	{
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumObject.IsLockedByProbeSnapshot))]
	public static bool IsLockedByProbeSnapshot(QuantumObject __instance, ref bool __result)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var worldObject = __instance.GetWorldObject<IQSBQuantumObject>();
		// TODO: this should just be equal to _visibleinprobesnapshot. check if theyre in sync and just go back to using that like we did before
		var visibleToProbePlayers = worldObject.GetVisibleToProbePlayers();
		__result = visibleToProbePlayers.Any();
		return false;
	}
}
