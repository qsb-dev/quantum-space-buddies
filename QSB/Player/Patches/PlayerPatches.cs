using QSB.Patches;

namespace QSB.Player.Patches
{
	internal class PlayerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches() => QSBCore.HarmonyHelper.AddPrefix<PlayerCrushedController>("CrushPlayer", typeof(PlayerPatches), nameof(PlayerCrushedController_CrushPlayer));
		public override void DoUnpatches() => QSBCore.HarmonyHelper.Unpatch<PlayerCrushedController>("CrushPlayer");

		public static bool PlayerCrushedController_CrushPlayer()
		{
			// #CrushIt https://www.twitch.tv/videos/846916781?t=00h03m51s
			// this is what you get from me when you mix tiredness and a headache - jokes and references only i will get
			Locator.GetDeathManager().KillPlayer(DeathType.Crushed);
			return false;
		}
	}
}
