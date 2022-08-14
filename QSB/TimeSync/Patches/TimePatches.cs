using HarmonyLib;
using QSB.Inputs;
using QSB.Patches;
using QSB.Utility;

namespace QSB.TimeSync.Patches;

[HarmonyPatch]
internal class TimePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.WakeUp))]
	public static void PlayerCameraEffectController_WakeUp(PlayerCameraEffectController __instance)
	{
		// prevent funny thing when you pause while waking up
		QSBInputManager.Instance.SetInputsEnabled(false);
		Delay.RunWhen(() => !__instance._isOpeningEyes, () => QSBInputManager.Instance.SetInputsEnabled(true));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(OWTime), nameof(OWTime.Pause))]
	public static bool StopPausing(OWTime.PauseType pauseType)
		=> pauseType
			is OWTime.PauseType.Initializing
			or OWTime.PauseType.Streaming
			or OWTime.PauseType.Loading;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SubmitActionSkipToNextLoop), nameof(SubmitActionSkipToNextLoop.AdvanceToNewTimeLoop))]
	public static bool StopMeditation()
		=> false;
}
