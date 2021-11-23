using HarmonyLib;
using QSB.Patches;
using QSB.Utility;

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
			DebugLog.DebugWrite($"{__instance.name} OnStartConversation");
			__instance._talking = true;

			if (__instance is GabbroTravelerController gabbro)
			{
				if (gabbro._animator.enabled)
				{
					gabbro._animator.CrossFadeInFixedTime("Gabbro_Talking", 1.8f);
					gabbro._hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Talking", 1.8f);
				}
				// Locator.GetTravelerAudioManager().StopAllTravelerAudio();
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
				// Locator.GetTravelerAudioManager().StopAllTravelerAudio();

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
			DebugLog.DebugWrite($"{__instance.name} OnEndConversation");

			if (__instance is GabbroTravelerController gabbro)
			{
				if (gabbro._animator.enabled)
				{
					gabbro._animator.CrossFadeInFixedTime("Gabbro_Playing", gabbro._delayToRestartAudio, -1, -gabbro._delayToRestartAudio);
					gabbro._hammockAnimator.CrossFadeInFixedTime("GabbroHammock_Playing", gabbro._delayToRestartAudio, -1, -gabbro._delayToRestartAudio);
				}
				// Locator.GetTravelerAudioManager().PlayAllTravelerAudio(gabbro._delayToRestartAudio);
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
				// Locator.GetTravelerAudioManager().PlayAllTravelerAudio(__instance._delayToRestartAudio);
			}

			__instance._talking = false;

			return false;
		}
	}
}
