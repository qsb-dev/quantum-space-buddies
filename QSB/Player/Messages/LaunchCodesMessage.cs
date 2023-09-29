using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Player.Messages;

public class LaunchCodesMessage : QSBMessage
{
	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var flag = false;
		if (!PlayerData._currentGameSave.PersistentConditionExists("LAUNCH_CODES_GIVEN"))
		{
			flag = true;
		}
		else if (!PlayerData._currentGameSave.GetPersistentCondition("LAUNCH_CODES_GIVEN"))
		{
			flag = true;
		}

		if (flag)
		{
			DialogueConditionManager.SharedInstance.SetConditionState("SCIENTIST_3", true);
			PlayerData._currentGameSave.SetPersistentCondition("LAUNCH_CODES_GIVEN", true);
			GlobalMessenger.FireEvent("LearnLaunchCodes");
		}
	}
}