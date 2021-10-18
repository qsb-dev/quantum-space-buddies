using HarmonyLib;
using QSB.Patches;
using System.Reflection;

namespace QSB.QuantumSync.Patches
{
	[HarmonyPatch]
	public class ClientQuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.Start))]
		public static void QuantumMoon_Start(QuantumMoon __instance)
			=> __instance.GetType().GetMethod("SetSurfaceState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { -1 });

		[HarmonyPrefix]
		[HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.ChangeQuantumState))]
		public static bool QuantumMoon_ChangeQuantumState()
			=> false;
	}
}