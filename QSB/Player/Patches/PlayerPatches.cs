using HarmonyLib;
using QSB.Events;
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

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.LearnLaunchCodes))]
		public static bool LearnLaunchCodes()
		{
			var flag = false;
			if (!PlayerData._currentGameSave.PersistentConditionExists("LAUNCH_CODES_GIVEN"))
			{
				flag = true;
			}

			else if (PlayerData._currentGameSave.GetPersistentCondition("LAUNCH_CODES_GIVEN"))
			{
				flag = true;
			}

			if (flag)
			{
				DialogueConditionManager.SharedInstance.SetConditionState("SCIENTIST_3", true);
				PlayerData._currentGameSave.SetPersistentCondition("LAUNCH_CODES_GIVEN", true);
				GlobalMessenger.FireEvent("LearnLaunchCodes");
				QSBEventManager.FireEvent(EventNames.QSBLearnLaunchCodes);
			}

			return false;
		}
	}
}
