using HarmonyLib;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Patches;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Ghosts.Patches;

[HarmonyPatch(typeof(GhostSensors))]
internal class GhostSensorsPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostSensors.Initialize))]
	public static bool Initialize(GhostSensors __instance, GhostData data, OWTriggerVolume guardVolume)
	{
		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostSensors.CanGrabPlayer))]
	public static bool CanGrabPlayer(GhostSensors __instance, ref bool __result)
	{
		DebugLog.ToConsole($"Error - {MethodBase.GetCurrentMethod().Name} not supported!", OWML.Common.MessageType.Error);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostSensors.FixedUpdate_Sensors))]
	public static bool FixedUpdate_Sensors(GhostSensors __instance)
	{
		__instance.GetWorldObject<QSBGhostSensors>().FixedUpdate_Sensors();
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostSensors.OnEnterContactTrigger))]
	public static bool OnEnterContactTrigger(GhostSensors __instance, GameObject hitObj)
	{
		__instance.GetWorldObject<QSBGhostSensors>().OnEnterContactTrigger(hitObj);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GhostSensors.OnExitContactTrigger))]
	public static bool OnExitContactTrigger(GhostSensors __instance, GameObject hitObj)
	{
		__instance.GetWorldObject<QSBGhostSensors>().OnExitContactTrigger(hitObj);
		return false;
	}
}
