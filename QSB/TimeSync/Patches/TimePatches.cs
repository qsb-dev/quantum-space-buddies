using HarmonyLib;
using QSB.Inputs;
using QSB.Messaging;
using QSB.Patches;
using QSB.TimeSync.Messages;
using QSB.Utility;
using UnityEngine;

namespace QSB.TimeSync.Patches;

[HarmonyPatch]
public class TimePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.OnStartOfTimeLoop))]
	public static bool PlayerCameraEffectController_OnStartOfTimeLoop(PlayerCameraEffectController __instance)
	{
		if (!QSBCore.IsInMultiplayer)
		{
			DebugLog.DebugWrite("NOT IN MULTIPLAYER");
			return true;
		}

		if (!QSBCore.IsHost)
		{
			// remove the wakeup prompt for clients - they are woken up automatically
			DebugLog.DebugWrite("NOT HOST");
			return false;
		}

		if (!__instance.gameObject.CompareTag("MainCamera")
		    || LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse)
		{
			DebugLog.DebugWrite("NOT IN SOLAR SYSTEM");
			return false;
		}

		// force the wakeup prompt to always appear for the host

		__instance._owCamera.postProcessingSettings.eyeMask.openness = 0f;
		__instance._owCamera.postProcessingSettings.bloom.threshold = 0f;
		__instance._owCamera.postProcessingSettings.eyeMaskEnabled = true;
		__instance._waitForWakeInput = true;
		__instance._wakePrompt = new ScreenPrompt(InputLibrary.interact, "Wake up\r\n(Players will not be able to join after this)", 0, ScreenPrompt.DisplayState.Normal, false);
		__instance._wakePrompt.SetVisibility(false);
		Locator.GetPromptManager().AddScreenPrompt(__instance._wakePrompt, PromptPosition.Center, false);
		OWTime.Pause(OWTime.PauseType.Sleeping);
		Locator.GetPauseCommandListener().AddPauseCommandLock();

		return false;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.WakeUp))]
	public static void PlayerCameraEffectController_WakeUp(PlayerCameraEffectController __instance)
	{
		if (!QSBCore.IsInMultiplayer)
		{
			return;
		}

		// prevent funny thing when you pause while waking up
		Locator.GetPauseCommandListener().AddPauseCommandLock();
		Delay.RunWhen(() => !__instance._isOpeningEyes, () => Locator.GetPauseCommandListener().RemovePauseCommandLock());
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(OWTime), nameof(OWTime.Pause))]
	public static bool StopPausing(OWTime.PauseType pauseType)
	{
		if (!QSBCore.IsInMultiplayer)
		{
			return true;
		}

		return pauseType
			is OWTime.PauseType.Initializing
			or OWTime.PauseType.Streaming
			or OWTime.PauseType.Loading
			|| (QSBCore.IsHost && pauseType == OWTime.PauseType.Sleeping);
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(SubmitActionSkipToNextLoop), nameof(SubmitActionSkipToNextLoop.AdvanceToNewTimeLoop))]
	public static void PreventMeditationSoftlock()
	{
		if (!QSBCore.IsInMultiplayer)
		{
			return;
		}

		OWInput.ChangeInputMode(InputMode.Character);
	}
}

public class ClientTimePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnNonServerClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TimeLoop), nameof(TimeLoop.SetSecondsRemaining))]
	private static void SetSecondsRemaining(float secondsRemaining)
	{
		if (Remote)
		{
			return;
		}
		new SetSecondsRemainingMessage(secondsRemaining).Send();
	}
}
