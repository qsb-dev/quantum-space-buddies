using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages;

public class DialogueConditionMessage : QSBMessage<(string Name, bool State)>
{
	public DialogueConditionMessage(string name, bool state) : base((name, state)) { }

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			QSBWorldSync.SetDialogueCondition(Data.Name, Data.State);
		}

		var sharedInstance = DialogueConditionManager.SharedInstance;

		var flag = true;
		if (sharedInstance.ConditionExists(Data.Name))
		{
			if (sharedInstance._dictConditions[Data.Name] == Data.State)
			{
				flag = false;
			}

			sharedInstance._dictConditions[Data.Name] = Data.State;
		}
		else
		{
			sharedInstance.AddCondition(Data.Name, Data.State);
		}

		if (flag)
		{
			GlobalMessenger<string, bool>.FireEvent("DialogueConditionChanged", Data.Name, Data.State);
		}

		if (Data.Name == "LAUNCH_CODES_GIVEN")
		{
			PlayerData.LearnLaunchCodes();
		}
	}

	public override void OnReceiveLocal()
	{
		if (QSBCore.IsHost)
		{
			QSBWorldSync.SetDialogueCondition(Data.Name, Data.State);
		}
	}
}