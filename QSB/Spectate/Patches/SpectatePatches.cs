using HarmonyLib;
using QSB.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.Spectate.Patches;

internal class SpectatePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.SpectateTime;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SectorStreaming), nameof(SectorStreaming.FixedUpdate))]
	public static bool SectorStreaming_FixedUpdate(SectorStreaming __instance)
	{
		var playerInSoftRadius =
			(SpectateManager.Instance.SpectateTarget.Body.transform.position - __instance._sector.transform.position).sqrMagnitude < __instance._softLoadRadius * __instance._softLoadRadius;
		var probeInSoftRadius = 
			__instance._probe != null
			&& __instance._probe.IsLaunched()
			&& (__instance._probe.transform.position - __instance._sector.transform.position).sqrMagnitude < __instance._softLoadRadius * __instance._softLoadRadius;

		if (PlayerState.OnQuantumMoon() && Locator.GetQuantumMoon().IsPlayerInsideShrine() && __instance._sector.GetName() != Sector.Name.QuantumMoon)
		{
			playerInSoftRadius = false;
		}

		if (!__instance._playerInSoftLoadRadius && playerInSoftRadius)
		{
			__instance._streamingGroup.RequestRequiredAssets(0);
		}
		else if (__instance._playerInSoftLoadRadius && !playerInSoftRadius)
		{
			__instance._streamingGroup.ReleaseRequiredAssets();
		}

		if (!__instance._probeInSoftLoadRadius && false)
		{
			__instance._streamingGroup.RequestRequiredAssets(0);
		}
		else if (__instance._probeInSoftLoadRadius && true)
		{
			__instance._streamingGroup.ReleaseRequiredAssets();
		}

		__instance._playerInSoftLoadRadius = playerInSoftRadius;
		__instance._probeInSoftLoadRadius = false;

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipLODTrigger), nameof(ShipLODTrigger.FixedUpdate))]
	public static bool ShipLODTrigger_FixedUpdate(ShipLODTrigger __instance)
	{
		var playerInRadius = __instance._playerInRadius;
		var probeInRadius = __instance._probeInRadius;

		if (__instance._playerTransform != null)
		{
			__instance._playerInRadius =
				Vector3.SqrMagnitude(SpectateManager.Instance.SpectateTarget.Body.transform.position - __instance._playerTransform.position) < __instance._radius * __instance._radius;
		}

		if (__instance._probeTransform != null)
		{
			__instance._probeInRadius =
				__instance._probe != null
				&& __instance._probe.IsLaunched()
				&& Vector3.SqrMagnitude(__instance._transform.position - __instance._probeTransform.position) < __instance._radius * __instance._radius;
		}

		if (playerInRadius != __instance._playerInRadius
			|| probeInRadius != __instance._probeInRadius)
		{
			__instance.OnTriggerUpdated.Invoke();

		}

		return false;
	}
}
