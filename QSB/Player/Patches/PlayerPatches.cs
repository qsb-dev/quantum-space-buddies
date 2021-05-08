using QSB.Patches;
using QSB.Utility;

namespace QSB.Player.Patches
{
	internal class PlayerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.HarmonyHelper.AddPrefix<PlayerCrushedController>("CrushPlayer", typeof(PlayerPatches), nameof(PlayerCrushedController_CrushPlayer));
			QSBCore.HarmonyHelper.AddPrefix<PauseMenuManager>("OnExitToMainMenu", typeof(PlayerPatches), nameof(PauseMenuManager_OnExitToMainMenu));
		}

		public override void DoUnpatches()
		{
			QSBCore.HarmonyHelper.Unpatch<PlayerCrushedController>("CrushPlayer");
			QSBCore.HarmonyHelper.Unpatch<PauseMenuManager>("OnExitToMainMenu");
		}

		public static bool PlayerCrushedController_CrushPlayer()
		{
			// #CrushIt https://www.twitch.tv/videos/846916781?t=00h03m51s
			// this is what you get from me when you mix tiredness and a headache - jokes and references only i will get
			Locator.GetDeathManager().KillPlayer(DeathType.Crushed);
			return false;
		}

		public static void PauseMenuManager_OnExitToMainMenu()
		{
			QSBPlayerManager.LocalPlayer.PlayerStates.IsReady = false;
		}
	}
}
