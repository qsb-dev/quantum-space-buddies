using QSB.Patches;

namespace QSB.Player.Patches
{
	internal class PlayerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			Prefix(nameof(PlayerCrushedController_CrushPlayer));
			Prefix(nameof(PauseMenuManager_OnExitToMainMenu));
		}

		public static bool PlayerCrushedController_CrushPlayer()
		{
			// #CrushIt https://www.twitch.tv/videos/846916781?t=00h03m51s
			// this is what you get from me when you mix tiredness and a headache - jokes and references only i will get
			Locator.GetDeathManager().KillPlayer(DeathType.Crushed);
			return false;
		}

		public static void PauseMenuManager_OnExitToMainMenu() => QSBPlayerManager.LocalPlayer.PlayerStates.IsReady = false;
	}
}
