using HarmonyLib;
using QSB.Patches;

namespace QSB.QuantumSync.Patches.Client;

[HarmonyPatch(typeof(QuantumMoon))]
internal class ClientQuantumMoonPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(QuantumMoon.Start))]
	public static void Start(QuantumMoon __instance)
		=> __instance.SetSurfaceState(-1);

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumMoon.ChangeQuantumState))]
	public static bool ChangeQuantumState()
		=> false;
}
