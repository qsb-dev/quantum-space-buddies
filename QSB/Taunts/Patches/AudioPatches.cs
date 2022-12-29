using HarmonyLib;
using QSB.Patches;
using QSB.TimeSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.Taunts.Patches;
internal class AudioPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TravelerAudioManager), nameof(TravelerAudioManager.SyncTravelers))]
	public static bool SyncTravelers(TravelerAudioManager __instance)
	{
		foreach (var signal in __instance._signals)
		{
			var timeToSet = WakeUpSync.LocalInstance.TimeSinceServerStart;
			timeToSet %= signal.GetOWAudioSource().clip.length;

			signal.GetOWAudioSource().time = timeToSet;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TravelerAudioManager), nameof(TravelerAudioManager.OnUnpause))]
	public static bool Unpause(TravelerAudioManager __instance)
	{
		foreach (var signal in __instance._signals)
		{
			var timeToSet = WakeUpSync.LocalInstance.TimeSinceServerStart;
			timeToSet %= signal.GetOWAudioSource().clip.length;

			signal.GetOWAudioSource().time = timeToSet;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TravelerAudioManager), nameof(TravelerAudioManager.Update))]
	public static bool Update(TravelerAudioManager __instance)
	{
		if (!__instance._playAfterDelay || !(Time.time >= __instance._playAudioTime))
		{
			return false;
		}

		DebugLog.DebugWrite($"PLAY AFTER DELAY");

		foreach (var signal in __instance._signals)
		{
			//var timeToSet = WakeUpSync.LocalInstance.TimeSinceServerStart;
			//timeToSet %= signal.GetOWAudioSource().clip.length;

			signal.GetOWAudioSource().FadeIn(0.5f);
			//signal.GetOWAudioSource().time = timeToSet;
		}

		__instance._playAfterDelay = false;

		return false;
	}
}
