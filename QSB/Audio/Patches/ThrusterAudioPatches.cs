using HarmonyLib;
using QSB.Audio.Messages;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.Audio.Patches;

internal class ThrusterAudioPatches : QSBPatch
{
	// Since we patch Start we do it when the mod starts, else it won't run
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ThrusterAudio), nameof(ThrusterAudio.Start))]
	public static void ThrusterAudio_Start(ThrusterAudio __instance)
	{
		__instance._rotationalSource.gameObject.AddComponent<QSBAudioSourceOneShotTracker>();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ThrusterAudio), nameof(ThrusterAudio.OnFireRotationalThruster))]
	public static void ThrusterAudio_OnFireRotationalThruster_Prefix(ThrusterAudio __instance) =>
		// First we reset in case no audio is actually played
		__instance._rotationalSource.gameObject.GetComponent<QSBAudioSourceOneShotTracker>()?.Reset();

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ThrusterAudio), nameof(ThrusterAudio.OnFireRotationalThruster))]
	public static void ThrusterAudio_OnFireRotationalThruster_Postfix(ThrusterAudio __instance)
	{
		if (__instance._rotationalSource.gameObject.TryGetComponent<QSBAudioSourceOneShotTracker>(out var tracker))
		{
			if (tracker.LastPlayed != AudioType.None)
			{
				if (__instance is ShipThrusterAudio)
				{
					new ShipThrusterAudioOneShotMessage(tracker.LastPlayed, tracker.Pitch, tracker.Volume).Send();
				}
			}
		}
	}
}
