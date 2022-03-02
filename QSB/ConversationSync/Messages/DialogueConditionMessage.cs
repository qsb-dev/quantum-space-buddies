using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages
{
	public class DialogueConditionMessage : QSBMessage<string, bool>
	{
		public DialogueConditionMessage(string name, bool state)
		{
			Value1 = name;
			Value2 = state;
		}

		public override void OnReceiveRemote()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.SetDialogueCondition(Value1, Value2);
			}

			var sharedInstance = DialogueConditionManager.SharedInstance;

			var flag = true;
			if (sharedInstance.ConditionExists(Value1))
			{
				if (sharedInstance._dictConditions[Value1] == Value2)
				{
					flag = false;
				}

				sharedInstance._dictConditions[Value1] = Value2;
			}
			else
			{
				sharedInstance.AddCondition(Value1, Value2);
			}

			if (flag)
			{
				GlobalMessenger<string, bool>.FireEvent("DialogueConditionChanged", Value1, Value2);
			}

			if (Value1 == "LAUNCH_CODES_GIVEN")
			{
				PlayerData.LearnLaunchCodes();
			}
		}

		public override void OnReceiveLocal()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.SetDialogueCondition(Value1, Value2);
			}
		}
	}
}