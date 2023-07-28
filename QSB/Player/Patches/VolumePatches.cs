using HarmonyLib;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player.Patches;

public class VolumePatches : QSBPatch
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
		// TODO: also do this with remote probes
		return hitObj.name is not ("REMOTE_PlayerDetector" or "REMOTE_CameraDetector");
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VanishVolume), nameof(VanishVolume.OnTriggerEnter))]
	public static bool PreventRemotePlayerEnter(object __instance, Collider hitCollider)
	{
		DebugLog.DebugWrite($"{__instance} funny prevent enter {hitCollider}");
		// TODO: also do this with remote probes
		return hitCollider.name is not ("REMOTE_PlayerDetector" or "REMOTE_CameraDetector");
	}
}
