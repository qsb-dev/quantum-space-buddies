using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player.Messages;

namespace QSB.Player.Patches;

[HarmonyPatch]
internal class PlayerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

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