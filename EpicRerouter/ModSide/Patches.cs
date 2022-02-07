using HarmonyLib;
using static EntitlementsManager;

namespace EpicRerouter.ModSide
{
	[HarmonyPatch(typeof(EpicPlatformManager))]
	public static class Patches
	{
		public static void Apply() => Harmony.CreateAndPatchAll(typeof(Patches));

		[HarmonyPrefix]
		[HarmonyPatch("instance", MethodType.Getter)]
		private static bool GetInstance()
		{
			Interop.Log("instance get called. return nothing");
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch("instance", MethodType.Setter)]
		private static bool SetInstance()
		{
			Interop.Log("instance set called. do nothing");
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch("platformInterface", MethodType.Getter)]
		private static bool GetPlatformInterface()
		{
			Interop.Log("platformInterface get called. return nothing");
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(EntitlementsManager), nameof(EntitlementsManager.IsDlcOwned))]
		private static bool IsDlcOwned(out AsyncOwnershipStatus __result)
		{
			__result = Interop.OwnershipStatus;
			Interop.Log($"ownership status = {__result}");
			return false;
		}
	}
}
