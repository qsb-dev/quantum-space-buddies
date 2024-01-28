using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.ShipSync.TransformSync;
using UnityEngine;

namespace QSB.ShipSync.Patches;

public class ShipAudioPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipThrusterAudio), nameof(ShipThrusterAudio.Update))]
	public static bool ShipThrusterAudio_Update(ShipThrusterAudio __instance)
	{
		if (!QSBPlayerManager.LocalPlayer.FlyingShip)
		{
			// Just copy pasted the original method with this one line changed
			Vector3 localAcceleration = ShipTransformSync.LocalInstance.ThrusterVariableSyncer.AccelerationSyncer.Value;
			localAcceleration.y *= 0.5f;
			localAcceleration.z *= 0.5f;
			Vector3 vector = __instance._thrusterModel.IsThrusterBankEnabled(ThrusterBank.Left) ? localAcceleration : Vector3.zero;
			vector.x = Mathf.Max(0f, vector.x);
			Vector3 vector2 = __instance._thrusterModel.IsThrusterBankEnabled(ThrusterBank.Right) ? localAcceleration : Vector3.zero;
			vector2.x = Mathf.Min(0f, vector2.x);
			float maxTranslationalThrust = __instance._thrusterModel.GetMaxTranslationalThrust();
			__instance.UpdateTranslationalSourceVolume(__instance._leftTranslationalSource, __instance._thrustToVolumeCurve.Evaluate(vector.magnitude / maxTranslationalThrust), !__instance._underwater);
			__instance.UpdateTranslationalSourceVolume(__instance._rightTranslationalSource, __instance._thrustToVolumeCurve.Evaluate(vector2.magnitude / maxTranslationalThrust), !__instance._underwater);
			__instance.UpdateTranslationalSourceVolume(__instance._leftUnderwaterSource, __instance._thrustToVolumeCurve.Evaluate(vector.magnitude / maxTranslationalThrust), __instance._underwater);
			__instance.UpdateTranslationalSourceVolume(__instance._rightUnderwaterSource, __instance._thrustToVolumeCurve.Evaluate(vector2.magnitude / maxTranslationalThrust), __instance._underwater);
			if (!__instance._thrustersFiring && !__instance._leftTranslationalSource.isPlaying && !__instance._rightTranslationalSource.isPlaying && !__instance._leftUnderwaterSource.isPlaying && !__instance._rightUnderwaterSource.isPlaying)
			{
				__instance.enabled = false;
			}

			return false;
		}
		else
		{
			return true;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TravelMusicController), nameof(TravelMusicController.Update))]
	public static bool TravelMusicController_Update(TravelMusicController __instance)
	{
		// only this line is changed
		__instance._isTraveling = PlayerState.IsInsideShip()
		                          && ShipManager.Instance.CurrentFlyer != uint.MaxValue
		                          && Locator.GetPlayerRulesetDetector().AllowTravelMusic();

		if (__instance._isTraveling && !__instance._wasTraveling)
		{
			__instance._audioSource.FadeIn(5f, false, false, 1f);
		}
		else if (!__instance._isTraveling && __instance._wasTraveling)
		{
			__instance._audioSource.FadeOut(5f, OWAudioSource.FadeOutCompleteAction.PAUSE, 0f);
		}

		__instance._wasTraveling = __instance._isTraveling;

		return false;
	}
}
