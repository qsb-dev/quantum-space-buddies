using HarmonyLib;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player.Patches;

internal class VolumePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FluidVolume), nameof(FluidVolume.OnEffectVolumeEnter))]
	public static void OnEffectVolumeEnter(FluidVolume __instance, GameObject hitObj)
	{
		var comp = hitObj.GetComponent<RemotePlayerFluidDetector>();
		if (comp != null)
		{
			comp.AddVolume(__instance);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FluidVolume), nameof(FluidVolume.OnEffectVolumeExit))]
	public static void OnEffectVolumeExit(FluidVolume __instance, GameObject hitObj)
	{
		var comp = hitObj.GetComponent<RemotePlayerFluidDetector>();
		if (comp != null)
		{
			comp.RemoveVolume(__instance);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(RingRiverFluidVolume), nameof(RingRiverFluidVolume.OnEffectVolumeEnter))]
	public static void OnEffectVolumeEnter(RingRiverFluidVolume __instance, GameObject hitObj)
	{
		var comp = hitObj.GetComponent<RemotePlayerFluidDetector>();
		if (comp != null)
		{
			comp.AddVolume(__instance);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ElectricityVolume), nameof(ElectricityVolume.OnEffectVolumeEnter))]
	[HarmonyPatch(typeof(DreamWarpVolume), nameof(DreamWarpVolume.OnEnterTriggerVolume))]
	[HarmonyPatch(typeof(NomaiWarpPlatform), nameof(NomaiWarpPlatform.OnEntry))]
	public static bool PreventRemotePlayerEnter(object __instance, GameObject hitObj)
	{
		DebugLog.DebugWrite($"{__instance} funny prevent enter {hitObj}");
		// this is a dogshit fix to a bug where this would ApplyShock to remote players,
		// which would actually apply the shock affects to the entire planet / sector
		//
		// TODO: also do this with remote probes
		return hitObj.name != "REMOTE_PlayerDetector";
	}
}
