using HarmonyLib;
using QSB.Patches;
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
	public static void OnEffectVolumeEnter(ElectricityVolume __instance, GameObject hitObj)
	{
		var hazardDetector = hitObj.GetComponent<HazardDetector>();
		if (hazardDetector != null)
		{
			if (hitObj.name == "REMOTE_PlayerDetector")
			{
				// this is a dogshit fix to a bug where this would ApplyShock to remote players,
				// which would actually apply the shock affects to the entire planet / sector
				return;
			}

			hazardDetector.AddVolume(__instance);
			var isInsulated = hazardDetector.IsInsulated();
			if (!isInsulated)
			{
				__instance.ApplyShock(hazardDetector);
			}

			if (!__instance._trackedDetectors.Exists((ElectricityVolume.TrackedHazardDetector x) => x.hazardDetector == hazardDetector))
			{
				var trackedHazardDetector = new ElectricityVolume.TrackedHazardDetector
				{
					hazardDetector = hazardDetector,
					lastShockTime = Time.timeSinceLevelLoad,
					wasInsulated = isInsulated
				};
				__instance._trackedDetectors.Add(trackedHazardDetector);
			}
			__instance.enabled = true;
		}
	}
}
