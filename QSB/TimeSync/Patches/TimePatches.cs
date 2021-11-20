using HarmonyLib;
using QSB.Patches;
using QSB.Utility;

namespace QSB.TimeSync.Patches
{
	[HarmonyPatch]
	internal class TimePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerCameraEffectController), nameof(PlayerCameraEffectController.OnStartOfTimeLoop))]
		public static bool PlayerCameraEffectController_OnStartOfTimeLoop()
		{
			DebugLog.DebugWrite($"OnStartOfTimeLoop");
			return false;
		}
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
