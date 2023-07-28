using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.HUD.Patches;

[HarmonyPatch(typeof(RulesetVolume))]
public class RulesetVolumePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(RulesetVolume.OnEffectVolumeEnter))]
	public static bool OnEffectVolumeEnter(RulesetVolume __instance, GameObject hitObj)
	{
		var baseDetector = hitObj.GetComponent<RulesetDetector>();
		var customDetector = hitObj.GetComponent<RemotePlayerRulesetDetector>();
		if (baseDetector != null)
		{
			baseDetector.AddVolume(__instance);
		}
		else if (customDetector != null)
		{
			customDetector.AddVolume(__instance);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(RulesetVolume.OnEffectVolumeExit))]
	public static bool OnEffectVolumeExit(RulesetVolume __instance, GameObject hitObj)
	{
		var baseDetector = hitObj.GetComponent<RulesetDetector>();
		var customDetector = hitObj.GetComponent<RemotePlayerRulesetDetector>();
		if (baseDetector != null)
		{
			baseDetector.RemoveVolume(__instance);
		}
		else if (customDetector != null)
		{
			customDetector.RemoveVolume(__instance);
		}

		return false;
	}
}
