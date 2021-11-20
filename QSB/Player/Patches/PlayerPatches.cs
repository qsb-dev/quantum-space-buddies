using HarmonyLib;
using QSB.Patches;

namespace QSB.Player.Patches
{
	[HarmonyPatch]
	internal class PlayerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerCrushedController), nameof(PlayerCrushedController.CrushPlayer))]
		public static bool PlayerCrushedController_CrushPlayer()
		{
			// #CrushIt https://www.twitch.tv/videos/846916781?t=00h03m51s
			// this is what you get from me when you mix tiredness and a headache - jokes and references only i will get
			Locator.GetDeathManager().KillPlayer(DeathType.Crushed);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PauseMenuManager), nameof(PauseMenuManager.OnExitToMainMenu))]
		public static void PauseMenuManager_OnExitToMainMenu()
			=> QSBPlayerManager.LocalPlayer.IsReady = false;
	}
}
