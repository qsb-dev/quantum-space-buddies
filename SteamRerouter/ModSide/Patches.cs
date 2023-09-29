using HarmonyLib;
using UnityEngine;

namespace SteamRerouter.ModSide;

[HarmonyPatch]
public static class Patches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(EntitlementsManager), nameof(EntitlementsManager.InitializeOnAwake))]
	private static bool EntitlementsManager_InitializeOnAwake(EntitlementsManager __instance)
	{
		Object.Destroy(__instance);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EntitlementsManager), nameof(EntitlementsManager.Start))]
	private static bool EntitlementsManager_Start() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EntitlementsManager), nameof(EntitlementsManager.OnDestroy))]
	private static bool EntitlementsManager_OnDestroy() => false;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EntitlementsManager), nameof(EntitlementsManager.IsDlcOwned))]
	private static bool EntitlementsManager_IsDlcOwned(out EntitlementsManager.AsyncOwnershipStatus __result)
	{
		__result = Interop.OwnershipStatus;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Achievements), nameof(Achievements.Earn))]
	private static bool Achievements_Earn(Achievements.Type type)
	{
		Interop.EarnAchivement(type);
		return false;
	}
}
