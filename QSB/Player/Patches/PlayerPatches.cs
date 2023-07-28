using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player.Messages;

namespace QSB.Player.Patches;

[HarmonyPatch]
public class PlayerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	/// <summary>
	/// this usually does a bunch of extra stuff before crushing the player.
	/// it's too much effort to revert all that when respawning.
	/// so we just don't do the extra stuff.
	/// </summary>
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
			new LaunchCodesMessage().Send();
		}

		return false;
	}
}