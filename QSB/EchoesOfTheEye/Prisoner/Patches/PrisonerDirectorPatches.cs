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
}
