using HarmonyLib;
using UnityEngine;
using static EntitlementsManager;

namespace SteamRerouter.ModSide;

[HarmonyPatch(typeof(EpicPlatformManager))]
public static class Patches
{
	public static void Apply()
	{
		var harmony = new Harmony(typeof(Patches).FullName);
		harmony.PatchAll(typeof(EntitlementsManagerPatches));
		harmony.PatchAll(typeof(EpicPlatformManagerPatches));
	}

	[HarmonyPatch(typeof(EntitlementsManager))]
	private static class EntitlementsManagerPatches
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(EntitlementsManager.InitializeOnAwake))]
		private static bool InitializeOnAwake(EntitlementsManager __instance)
		{
			Object.Destroy(__instance);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(EntitlementsManager.Start))]
		private static bool Start() => false;

		[HarmonyPrefix]
		[HarmonyPatch(nameof(EntitlementsManager.OnDestroy))]
		private static bool OnDestroy() => false;

		[HarmonyPrefix]
		[HarmonyPatch(nameof(EntitlementsManager.IsDlcOwned))]
		private static bool IsDlcOwned(out AsyncOwnershipStatus __result)
		{
			__result = Interop.OwnershipStatus;
			Interop.Log($"ownership status = {__result}");
			return false;
		}
	}

	[HarmonyPatch(typeof(EpicPlatformManager))]
	private static class EpicPlatformManagerPatches
	{
		[HarmonyPrefix]
		[HarmonyPatch("Awake")]
		private static bool Awake(EpicPlatformManager __instance)
		{
			Object.Destroy(__instance);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch("Start")]
		private static bool Start() => false;

		[HarmonyPrefix]
		[HarmonyPatch("OnDestroy")]
		private static bool OnDestroy() => false;
	}
}