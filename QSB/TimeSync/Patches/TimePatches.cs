using HarmonyLib;
using QSB.Patches;

namespace QSB.TimeSync.Patches
{
	[HarmonyPatch]
	internal class TimePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.OnStartOfTimeLoop))]
		public static bool PlayerCameraEffectController_OnStartOfTimeLoop()
			=> false;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(OWTime), nameof(OWTime.Pause))]
		public static bool StopPausing()
			=> false;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SubmitActionSkipToNextLoop), nameof(SubmitActionSkipToNextLoop.AdvanceToNewTimeLoop))]
		public static bool StopMeditation()
			=> false;
	}
}
