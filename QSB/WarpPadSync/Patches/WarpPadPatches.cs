using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;
using QSB.WarpPadSync.Messages;
using QSB.WarpPadSync.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.WarpPadSync.Patches;

public class WarpPadPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	/*
	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiWarpPlatform), nameof(NomaiWarpPlatform.OpenBlackHole))]
	private static void NomaiWarpPlatform_OpenBlackHole(NomaiWarpPlatform __instance,
		NomaiWarpPlatform linkedPlatform, bool stayOpen = false)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}
		__instance.GetWorldObject<QSBWarpPad>().SendMessage(new OpenCloseMessage(true, linkedPlatform));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiWarpPlatform), nameof(NomaiWarpPlatform.CloseBlackHole))]
	private static void NomaiWarpPlatform_CloseBlackHole(NomaiWarpPlatform __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}
		__instance.GetWorldObject<QSBWarpPad>().SendMessage(new OpenCloseMessage(false, __instance._linkedPlatform));
	}
	*/

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiWarpPlatform), nameof(NomaiWarpPlatform.OnEntry))]
	private static void NomaiWarpPlatform_OnEntry(NomaiWarpPlatform __instance, GameObject hitObj)
	{
		DebugLog.DebugWrite($"{__instance}.OnEntry({hitObj})");
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiWarpPlatform), nameof(NomaiWarpPlatform.OnExit))]
	private static void NomaiWarpPlatform_OnExit(NomaiWarpPlatform __instance, GameObject hitObj)
	{
		DebugLog.DebugWrite($"{__instance}.OnExit({hitObj})");
	}
}
