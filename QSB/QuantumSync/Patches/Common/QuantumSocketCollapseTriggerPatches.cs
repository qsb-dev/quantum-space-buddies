using HarmonyLib;
using QSB.Patches;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(QuantumSocketCollapseTrigger))]
public class QuantumSocketCollapseTriggerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumSocketCollapseTrigger.OnTriggerEnter))]
	public static bool OnTriggerEnter() => false;
}
