using HarmonyLib;
using QSB.Audio.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;

namespace QSB.Audio.Patches;

public class PlayerImpactAudioPatches : QSBPatch
{
	// Since we patch Start we do it when the mod starts, else it won't run
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerImpactAudio), nameof(PlayerImpactAudio.Start))]
	public static void PlayerImpactAudio_Start(PlayerImpactAudio __instance)
	{
		__instance.gameObject.AddComponent<QSBAudioSourceOneShotTracker>();
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlayerImpactAudio), nameof(PlayerImpactAudio.OnImpact))]
	public static void PlayerImpactAudio_OnImpact_Prefix(PlayerImpactAudio __instance) =>
		// First we reset in case no audio is actually played
		__instance.gameObject.GetComponent<QSBAudioSourceOneShotTracker>()?.Reset();

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerImpactAudio), nameof(PlayerImpactAudio.OnImpact))]
	public static void PlayerImpactAudio_OnImpact_Postfix(PlayerImpactAudio __instance)
	{
		var tracker = __instance.gameObject.GetComponent<QSBAudioSourceOneShotTracker>();
		if (tracker)
		{
			if (tracker.LastPlayed != AudioType.None)
			{
				new PlayerAudioControllerOneShotMessage(tracker.LastPlayed, QSBPlayerManager.LocalPlayerId, tracker.Pitch, tracker.Volume).Send();
			}
		}
	}
}
