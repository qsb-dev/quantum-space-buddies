using HarmonyLib;
using QSB.Patches;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.Animation.NPC.Patches
{
	[HarmonyPatch(typeof(TravelerController))]
	public class TravelerControllerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(nameof(TravelerController.OnStartConversation))]
		public static bool OnStartConversation(TravelerController __instance)
		{
			__instance._talking = true;

			if (__instance is GabbroTravelerController gabbro)
			{
				if (gabbro._animator.enabled)
				{
					gabbro._animator.CrossFadeInFixedTime("Gabbro_Talking", 1.8f);
					gabbro._hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Talking", 1.8f);
				}
				Locator.GetTravelerAudioManager().StopTravelerAudio(gabbro.name);
			}
			else
			{
				if (__instance._animator != null && __instance._animator.enabled)
				{
					__instance._playingAnimID = __instance._animator.IsInTransition(0)
						? __instance._animator.GetNextAnimatorStateInfo(0).fullPathHash
						: __instance._animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

					__instance._animator.SetTrigger("Talking");
				}
				Locator.GetTravelerAudioManager().StopTravelerAudio(__instance.name);

				if (__instance is ChertTravelerController chert)
				{
					chert._moodWeight = (float)chert._mood;
				}
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(TravelerController.OnEndConversation))]
		public static bool OnEndConversation(TravelerController __instance)
		{
			if (__instance is GabbroTravelerController gabbro)
			{
				if (gabbro._animator.enabled)
				{
					gabbro._animator.CrossFadeInFixedTime("Gabbro_Playing", gabbro._delayToRestartAudio, -1, -gabbro._delayToRestartAudio);
					gabbro._hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Playing", gabbro._delayToRestartAudio, -1, -gabbro._delayToRestartAudio);
				}
				Locator.GetTravelerAudioManager().PlayTravelerAudio(gabbro.name, gabbro._delayToRestartAudio);
				if (DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_REMINDER") || DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_ATTENTION"))
				{
					var conditionState = DialogueConditionManager.SharedInstance.GetConditionState("MAP_PROMPT_ATTENTION");
					DialogueConditionManager.SharedInstance.SetConditionState("MAP_PROMPT_REMINDER");
					DialogueConditionManager.SharedInstance.SetConditionState("MAP_PROMPT_ATTENTION");
					GlobalMessenger<bool>.FireEvent("TriggerMapPromptReminder", conditionState);
				}
			}
			else
			{
				if (__instance._animator != null && __instance._animator.enabled)
				{
					if (__instance._delayToRestartAudio > 0f)
					{
						__instance._animator.CrossFadeInFixedTime(__instance._playingAnimID, __instance._delayToRestartAudio, -1, -__instance._delayToRestartAudio);
					}
					else
					{
						__instance._animator.SetTrigger("Playing");
					}
				}
				Locator.GetTravelerAudioManager().PlayTravelerAudio(__instance.name, __instance._delayToRestartAudio);
			}

			__instance._talking = false;

			return false;
		}
	}

	internal static class TravelerAudioManagerExtensions
	{
		/// bad, but works great
		private static SignalName TravelerToSignal(string name)
		{
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

			throw new ArgumentOutOfRangeException(nameof(name), name, null);
		}

		internal static void StopTravelerAudio(this TravelerAudioManager manager, string name)
		{
			var signalName = TravelerToSignal(name);
			var audioSignal = manager._signals.First(x => x.GetName() == signalName);

			audioSignal.GetOWAudioSource().FadeOut(0.5f);
		}

		internal static void PlayTravelerAudio(this TravelerAudioManager manager, string name, float audioDelay)
		{
			var signalName = TravelerToSignal(name);
			var audioSignal = manager._signals.First(x => x.GetName() == signalName);

			manager._playAfterDelay = false;
			manager._playAudioTime = Time.time + audioDelay;
			QSBCore.UnityEvents.RunWhen(() => Time.time >= manager._playAudioTime, () =>
			{
				if (!audioSignal.IsOnlyAudibleToScope() || audioSignal.GetOWAudioSource().isPlaying)
				{
					audioSignal.GetOWAudioSource().FadeIn(0.5f);
					audioSignal.GetOWAudioSource().timeSamples = 0;
				}
			});
		}
	}
}
