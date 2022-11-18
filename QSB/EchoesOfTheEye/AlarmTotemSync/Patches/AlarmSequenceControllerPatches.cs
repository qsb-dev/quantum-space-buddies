using HarmonyLib;
using QSB.Patches;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Patches;

[HarmonyPatch(typeof(AlarmSequenceController))]
public class AlarmSequenceControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(AlarmSequenceController.IncreaseAlarmCounter))]
	private static bool IncreaseAlarmCounter(AlarmSequenceController __instance)
	{
		__instance._alarmCounter++;
		if (__instance._alarmCounter == 1)
		{
			__instance.PlayChimes();
		}
		__instance.enabled = true;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(AlarmSequenceController.DecreaseAlarmCounter))]
	private static bool DecreaseAlarmCounter(AlarmSequenceController __instance)
	{
		__instance._alarmCounter--;
		if (__instance._alarmCounter < 0)
		{
			__instance._alarmCounter = 0;
			Debug.LogError("Something went wrong, alarm counter should never drop below zero!");
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(AlarmSequenceController.StopChimes))]
	private static bool StopChimes(AlarmSequenceController __instance)
	{
		__instance._playing = false;
		__instance._stopRequested = false;
		__instance._animationStarted = false;
		foreach (var alarmBell in AlarmTotemManager.AlarmBells)
		{
			alarmBell.StopAnimation();
		}
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(AlarmSequenceController.PlaySingleChime))]
	private static bool PlaySingleChime(AlarmSequenceController __instance)
	{
		foreach (var alarmBell in AlarmTotemManager.AlarmBells)
		{
			alarmBell.PlaySingleChime(__instance._chimeIndex);
		}
		if (!__instance._animationStarted && !__instance._dreamWorldController.IsInDream())
		{
			foreach (var alarmBell in AlarmTotemManager.AlarmBells)
			{
				alarmBell.PlayAnimation();
			}
			__instance._animationStarted = true;
		}
		if (__instance._dreamWorldController.IsInDream() && !__instance._dreamWorldController.IsExitingDream())
		{
			Locator.GetDreamWorldAudioController().PlaySingleAlarmChime(__instance._chimeIndex, __instance._volumeCurve.Evaluate(__instance._wakeFraction));
		}
		return false;
	}
}
