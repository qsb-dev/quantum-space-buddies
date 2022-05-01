using HarmonyLib;
using QSB.EchoesOfTheEye.Prisoner.Messages;
using QSB.Messaging;
using QSB.Patches;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner.Patches;

[HarmonyPatch(typeof(PrisonerDirector))]
public class PrisonerDirectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonerDirector.OnEnterEmergeTrigger))]
	public static bool OnEnterEmergeTrigger(PrisonerDirector __instance, GameObject hitObj)
	{
		if (__instance._darknessAwoken)
		{
			return false;
		}

		if (hitObj.CompareTag("PlayerDetector"))
		{
			__instance._darknessAwoken = true;
			__instance._prisonerBrain.BeginBehavior(PrisonerBehavior.Emerge, 0f);
			__instance._cellevator.OnPrisonerReveal();
			__instance._musicSource.SetLocalVolume(Locator.GetAudioManager().GetAudioEntry(__instance._musicSource.audioLibraryClip).volume);
			__instance._musicSource.Play();
			new EmergeTriggerMessage().Send();
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonerDirector.OnMindProjectionComplete))]
	public static bool OnMindProjectionComplete(PrisonerDirector __instance)
	{
		if (__instance._projectingVision)
		{
			__instance._visionTorchItem.mindProjectorTrigger.SetProjectorActive(false);
			__instance._projectingVision = false;
			__instance._prisonerBrain.BeginBehavior(PrisonerBehavior.OfferTorch, 3f);

			new MindProjectionCompleteMessage().Send();
		}

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(nameof(PrisonerDirector.OnStartProjectingOnPrisoner))]
	public static void OnStartProjectingOnPrisoner()
		=> new ProjectMessage().Send();

	[HarmonyPostfix]
	[HarmonyPatch(nameof(PrisonerDirector.OnStopProjectingOnPrisoner))]
	public static void OnStopProjectingOnPrisoner(PrisonerDirector __instance)
		=> new StopProjectMessage(__instance._visionTorchItem.mindSlideProjector.mindSlideCollection.slideCollectionContainer.isEndOfSlide).Send();

	[HarmonyPrefix]
	[HarmonyPatch(nameof(PrisonerDirector.OnEnterLightsOutTrigger))]
	public static bool OnEnterLightsOutTrigger(PrisonerDirector __instance, GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			__instance._lightsOutTrigger.OnEntry -= __instance.OnEnterLightsOutTrigger;
			__instance._prisonLighting.FadeTo(0f, 1f);
			__instance._hangingLampSource.PlayOneShot(AudioType.Candle_Extinguish, 1f);
			__instance._lightsOnAudioVolume.SetVolumeActivation(false);

			new EnterLightsOutMessage().Send();
		}

		return false;
	}
}
