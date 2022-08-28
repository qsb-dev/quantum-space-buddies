using HarmonyLib;
using QSB.Audio.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;

namespace QSB.Audio.Patches;

[HarmonyPatch]
internal class PlayerAudioControllerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	private static void PlayOneShot(AudioType audioType) =>
		new PlayerAudioControllerOneShotMessage(audioType, QSBPlayerManager.LocalPlayerId).Send();

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.PlayMarshmallowEat))]
	public static void PlayerAudioController_PlayMarshmallowEat() => PlayOneShot(AudioType.ToolMarshmallowEat);

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.PlayMarshmallowEatBurnt))]
	public static void PlayerAudioController_PlayMarshmallowEatBurnt() => PlayOneShot(AudioType.ToolMarshmallowEatBurnt);

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.PlayPatchPuncture))]
	public static void PlayerAudioController_PlayPatchPuncture() => PlayOneShot(AudioType.PlayerSuitPatchPuncture);

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.PlayMedkit))]
	public static void PlayerAudioController_PlayMedkit() => PlayOneShot(AudioType.ShipCabinUseMedkit);

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.PlayRefuel))]
	public static void PlayerAudioController_PlayRefuel() => PlayOneShot(AudioType.ShipCabinUseRefueller);
}