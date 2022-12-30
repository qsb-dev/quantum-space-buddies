using HarmonyLib;
using QSB.Patches;
using QSB.TimeSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Animation.NPC.Patches;

public class TravelerControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TravelerController), nameof(TravelerController.OnStartConversation))]
	public static bool OnStartConversation(TravelerController __instance)
	{
		__instance._talking = true;
		// call directly instead of firing event
		__instance.StartConversation();

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TravelerController), nameof(TravelerController.OnEndConversation))]
	public static bool OnEndConversation(TravelerController __instance)
	{
		// call directly instead of firing event
		__instance.EndConversation(__instance._delayToRestartAudio);
		__instance._talking = false;

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TravelerController), nameof(TravelerController.StartConversation))]
	public static bool StartConversation(TravelerController __instance)
	{
		if (__instance._animator != null && __instance._animator.enabled)
		{
			__instance._playingAnimID = __instance._animator.IsInTransition(0)
				? __instance._animator.GetNextAnimatorStateInfo(0).fullPathHash
				: __instance._animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
			__instance._animator.SetTrigger("Talking");
		}

		Locator.GetTravelerAudioManager().StopTravelerAudio(__instance);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GabbroTravelerController), nameof(GabbroTravelerController.StartConversation))]
	public static bool StartConversation(GabbroTravelerController __instance)
	{
		if (__instance._animator.enabled)
		{
			__instance._animator.CrossFadeInFixedTime("Gabbro_Talking", 1.8f);
			__instance._hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Talking", 1.8f);
		}

		Locator.GetTravelerAudioManager().StopTravelerAudio(__instance);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TravelerController), nameof(TravelerController.EndConversation))]
	public static bool EndConversation(TravelerController __instance, float audioDelay)
	{
		if (__instance._animator != null && __instance._animator.enabled)
		{
			if (audioDelay > 0f)
			{
				__instance._animator.CrossFadeInFixedTime(__instance._playingAnimID, audioDelay, -1, -audioDelay);
			}
			else
			{
				__instance._animator.SetTrigger("Playing");
			}
		}

		Locator.GetTravelerAudioManager().PlayTravelerAudio(__instance, audioDelay);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(GabbroTravelerController), nameof(GabbroTravelerController.EndConversation))]
	public static bool EndConversation(GabbroTravelerController __instance, float audioDelay)
	{
		if (__instance._animator.enabled)
		{
			__instance._animator.CrossFadeInFixedTime("Gabbro_Playing", audioDelay, -1, -audioDelay);
			__instance._hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Playing", audioDelay, -1, -audioDelay);
		}

		Locator.GetTravelerAudioManager().PlayTravelerAudio(__instance, audioDelay);
		if (DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_REMINDER") || DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_ATTENTION"))
		{
			var conditionState = DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_ATTENTION");
			DialogueConditionManager.SharedInstance.SetConditionState("MAP_PROMPT_REMINDER");
			DialogueConditionManager.SharedInstance.SetConditionState("MAP_PROMPT_ATTENTION");
			GlobalMessenger<bool>.FireEvent("TriggerMapPromptReminder", conditionState);
		}

		return false;
	}
}

internal static class TravelerAudioManagerExtensions
{
	/// bad, but works great
	private static SignalName? TravelerToSignalName(TravelerController traveler)
	{
		var name = traveler.name;

		if (name.Contains("Esker"))
		{
			return SignalName.Traveler_Esker;
		}

		if (name.Contains("Chert"))
		{
			return SignalName.Traveler_Chert;
		}

		if (name.Contains("Riebeck"))
		{
			return SignalName.Traveler_Riebeck;
		}

		if (name.Contains("Gabbro"))
		{
			return SignalName.Traveler_Gabbro;
		}

		if (name.Contains("Feldspar"))
		{
			return SignalName.Traveler_Feldspar;
		}

		if (name.Contains("Nomai"))
		{
			return SignalName.Traveler_Nomai;
		}

		if (name.Contains("Prisoner"))
		{
			return SignalName.Traveler_Prisoner;
		}

		return null;
	}

	internal static void StopTravelerAudio(this TravelerAudioManager manager, TravelerController traveler)
	{
		var signalName = TravelerToSignalName(traveler);
		if (signalName == null)
		{
			return;
		}

		var signals = manager._signals.Where(x => x.GetName() == signalName);

		foreach (var signal in signals)
		{
			signal.GetOWAudioSource().FadeOut(0.5f);
		}
	}

	internal static void PlayTravelerAudio(this TravelerAudioManager manager, TravelerController traveler, float audioDelay)
	{
		var signalName = TravelerToSignalName(traveler);
		if (signalName == null)
		{
			return;
		}

		var signals = manager._signals.Where(x => x.GetName() == signalName);

		manager._playAfterDelay = false;
		manager._playAudioTime = Time.time + audioDelay;
		Delay.RunWhen(() => Time.time >= manager._playAudioTime, () =>
		{
			foreach (var signal in signals)
			{
				if (!signal.IsOnlyAudibleToScope() || signal.GetOWAudioSource().isPlaying)
				{
					signal.GetOWAudioSource().FadeIn(0.5f);
					signal.GetOWAudioSource().timeSamples = 0;
				}
			}
		});
	}
}