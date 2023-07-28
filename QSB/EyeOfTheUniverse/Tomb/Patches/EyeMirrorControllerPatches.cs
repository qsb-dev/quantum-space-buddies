using HarmonyLib;
using QSB.EyeOfTheUniverse.Tomb.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.Tomb.Patches;

[HarmonyPatch(typeof(EyeMirrorController))]
public class EyeMirrorControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(EyeMirrorController.OnSwapPortrait))]
	public static void OnSwapPortrait(EyeMirrorController __instance)
	{
		DebugLog.DebugWrite($"_numSwappedPortraits is now {__instance._numSwappedPortraits}. _portrait.Length is {__instance._portraits.Length}");
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(EyeMirrorController.OnEnterCloseDoorTrigger))]
	public static bool OnEnterCloseDoorTrigger(EyeMirrorController __instance, GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			__instance._door.Close();
			new CloseDoorMessage().Send();
		}

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(EyeMirrorController.OnLitStateChanged))]
	public static void OnLitStateChanged()
	{
		QSBPlayerManager.HideAllPlayers(0.5f);
	}
}
