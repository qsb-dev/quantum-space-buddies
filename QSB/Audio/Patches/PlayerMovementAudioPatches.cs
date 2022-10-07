using HarmonyLib;
using QSB.Audio.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;

namespace QSB.Audio.Patches;

internal class PlayerMovementAudioPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerMovementAudio), nameof(PlayerMovementAudio.PlayFootstep))]
	public static void PlayerMovementAudio_PlayFootstep(PlayerMovementAudio __instance)
	{
		var underwater = !PlayerState.IsCameraUnderwater() && __instance._fluidDetector.InFluidType(FluidVolume.Type.WATER);
		var audioType = underwater ? AudioType.MovementShallowWaterFootstep : PlayerMovementAudio.GetFootstepAudioType(__instance._playerController.GetGroundSurface());

		if (audioType != AudioType.None)
		{
			new PlayerMovementAudioFootstepMessage(audioType, __instance._footstepAudio.pitch, QSBPlayerManager.LocalPlayerId).Send();
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlayerMovementAudio), nameof(PlayerMovementAudio.OnJump))]
	public static void PlayerMovementAudio_OnJump(PlayerMovementAudio __instance)
	{
		new PlayerMovementAudioJumpMessage(__instance._jumpAudio.pitch, QSBPlayerManager.LocalPlayerId).Send();
	}
}