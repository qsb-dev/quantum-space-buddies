using HarmonyLib;
using QSB.Patches;

namespace QSB.QuantumSync.Patches
{
	[HarmonyPatch]
	public class ClientQuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.Start))]
		public static void QuantumMoon_Start(QuantumMoon __instance)
			=> __instance.SetSurfaceState(-1);

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.ChangeQuantumState))]
		public static bool QuantumMoon_ChangeQuantumState()
			=> false;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EyeProxyQuantumMoon), nameof(EyeProxyQuantumMoon.ChangeQuantumState))]
		public static bool EyeProxyQuantumMoon_ChangeQuantumState()
			=> false;
	}
}