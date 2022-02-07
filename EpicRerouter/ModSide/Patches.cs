using HarmonyLib;
using static EntitlementsManager;

namespace EpicRerouter.ModSide
{
	[HarmonyPatch(typeof(EpicPlatformManager))]
	public static class Patches
	{
		public static void Apply() => Harmony.CreateAndPatchAll(typeof(Patches));

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
