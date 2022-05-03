using HarmonyLib;
using QSB.Patches;

namespace QSB.Tools.ProbeTool.Patches;

internal class ProbeToolPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	/*
	 * This patch is just to avoid error spam when testing probe destruction in SolarSystem
	 */

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiWarpStreaming), nameof(NomaiWarpStreaming.FixedUpdate))]
	public static bool FixedUpdateOverride(NomaiWarpStreaming __instance)
	{
		if (__instance._warpTransmitter != null)
		{
			var ableToBeWarped = __instance._warpTransmitter.GetViewAngleToTarget() < __instance._streamingAngle;
			var probeAbove = __instance._probe != null && __instance._probe.IsLaunched() && (!__instance._probe.IsAnchored() || __instance._warpTransmitter.IsProbeOnPlatform());

			var shouldBeLoadingRequiredAssets = ableToBeWarped && (__instance._playerInVolume || (__instance._probeInVolume && probeAbove));
			var shouldBeLoadingGeneralAssets = ableToBeWarped && __instance._warpTransmitter.IsPlayerOnPlatform();
			__instance.UpdatePreloadingState(shouldBeLoadingRequiredAssets, shouldBeLoadingGeneralAssets);
		}

		if (__instance._warpReceiver != null)
		{
			var ableToBeWarped = __instance._warpReceiver.IsReturnWarpEnabled() || __instance._warpReceiver.IsBlackHoleOpen();
			var probeAbove = __instance._probe != null && __instance._probe.IsLaunched() && (!__instance._probe.IsAnchored() || __instance._warpReceiver.IsProbeOnPlatform());

			var shouldBeLoadingRequiredAssets = ableToBeWarped && (__instance._playerInVolume || (__instance._probeInVolume && probeAbove));
			var shouldBeLoadingGeneralAssets = ableToBeWarped && __instance._playerInVolume;
			__instance.UpdatePreloadingState(shouldBeLoadingRequiredAssets, shouldBeLoadingGeneralAssets);
		}

		return false;
	}
}