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

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.OnArtifactFocus))]
	public static void PlayerAudioController_OnArtifactFocus() => PlayOneShot(AudioType.Artifact_Focus);

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.OnArtifactUnfocus))]
	public static void PlayerAudioController_OnArtifactUnfocus() => PlayOneShot(AudioType.Artifact_Unfocus);

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.OnArtifactConceal))]
	public static void PlayerAudioController_OnArtifactConceal() => PlayOneShot(AudioType.Artifact_Conceal);

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerAudioController), nameof(PlayerAudioController.OnArtifactUnconceal))]
	public static void PlayerAudioController_OnArtifactUnconceal() => PlayOneShot(AudioType.Artifact_Unconceal);
}