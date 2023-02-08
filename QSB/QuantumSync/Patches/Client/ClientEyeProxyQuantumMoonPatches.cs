using HarmonyLib;
using QSB.Patches;

namespace QSB.QuantumSync.Patches.Client;

[HarmonyPatch(typeof(EyeProxyQuantumMoon))]
internal class ClientEyeProxyQuantumMoonPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(EyeProxyQuantumMoon.ChangeQuantumState))]
	public static bool ChangeQuantumState()
		=> false;
}
