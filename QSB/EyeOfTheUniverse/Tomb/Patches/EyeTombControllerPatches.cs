using HarmonyLib;
using QSB.EyeOfTheUniverse.Tomb.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.Tomb.Patches;

[HarmonyPatch(typeof(EyeTombController))]
public class EyeTombControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(EyeTombController.OnPressInteract))]
	public static void OnPressInteract() => new UseTombMessage(true).Send();

	[HarmonyPostfix]
	[HarmonyPatch(nameof(EyeTombController.CancelInteraction))]
	public static void CancelInteract() => new UseTombMessage(false).Send();

	[HarmonyPrefix]
	[HarmonyPatch(nameof(EyeTombController.OnEnterStage))]
	public static bool OnEnterStage(EyeTombController __instance, GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			__instance._candleController.FadeTo(1, 1);
			new EnterExitStageMessage(true).Send();
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(EyeTombController.OnExitStage))]
	public static bool OnExitStage(EyeTombController __instance, GameObject hitObj)
	{
		if (hitObj.CompareTag("PlayerDetector"))
		{
			__instance._candleController.FadeTo(0, 1);
			new EnterExitStageMessage(false).Send();
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(EyeTombController.ToggleLitState))]
	public static bool ToggleLitState(EyeTombController __instance, int direction)
	{
		var newStateIndex = __instance._stateIndex + direction;
		if (__instance._lit && (newStateIndex < 0 || newStateIndex > __instance._states.Length - 1))
		{
			__instance._gearEffects.PlayFailure(direction > 0, 1f);
			return false;
		}

		new ToggleLitStateMessage(__instance._stateIndex, direction, __instance._lit).Send();

		__instance._lit = !__instance._lit;
		__instance._planetLightController.SetIntensity(__instance._lit ? 1f : 0f);
		__instance._planetObject.SetActive(__instance._lit);
		__instance._lightBeamController.SetFade(__instance._lit ? 1f : 0f);
		if (!__instance._lit)
		{
			__instance._states[__instance._stateIndex].SetActive(false);
			__instance._stateIndex += direction;
			__instance._states[__instance._stateIndex].SetActive(true);
		}

		__instance._gearEffects.AddRotation(direction * 45f, 0f);
		__instance._oneShotSource.PlayOneShot((direction > 0f) ? AudioType.Projector_Next : AudioType.Projector_Prev, 1f);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(EyeTombController.OnObserveGrave))]
	public static bool OnObserveGrave()
		=> false;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(EyeTombController.OnFinishGather))]
	public static void OnFinishGather()
		=> QSBPlayerManager.ShowAllPlayers(0.5f);
}
