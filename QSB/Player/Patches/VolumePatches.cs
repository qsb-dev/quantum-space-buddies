using HarmonyLib;
using QSB.Patches;
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
	[HarmonyPatch(typeof(NomaiWarpReceiver), nameof(NomaiWarpReceiver.OnEntry))]
	public static bool PreventRemotePlayerEnter(object __instance, GameObject hitObj)
		=> hitObj.name is not ("REMOTE_PlayerDetector" or "REMOTE_CameraDetector" or "REMOTE_ProbeDetector");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VanishVolume), nameof(VanishVolume.OnTriggerEnter))]
	public static bool PreventRemotePlayerEnter(object __instance, Collider hitCollider)
		=> hitCollider.name is not ("REMOTE_PlayerDetector" or "REMOTE_CameraDetector" or "REMOTE_ProbeDetector");
}
