﻿using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.ConversationSync.Messages
{
	public class DialogueConditionMessage : QSBMessage
	{
		private string ConditionName;
		private bool ConditionState;

		public DialogueConditionMessage(string name, bool state)
		{
			ConditionName = name;
			ConditionState = state;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ConditionName);
			writer.Write(ConditionState);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ConditionName = reader.ReadString();
			ConditionState = reader.ReadBoolean();
		}

		public override void OnReceiveRemote()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.SetDialogueCondition(ConditionName, ConditionState);
			}

			var sharedInstance = DialogueConditionManager.SharedInstance;

			var flag = true;
			if (sharedInstance.ConditionExists(ConditionName))
			{
				if (sharedInstance._dictConditions[ConditionName] == ConditionState)
				{
					flag = false;
				}

				sharedInstance._dictConditions[ConditionName] = ConditionState;
			}
			else
			{
				sharedInstance.AddCondition(ConditionName, ConditionState);
			}

			if (flag)
			{
				GlobalMessenger<string, bool>.FireEvent("DialogueConditionChanged", ConditionName, ConditionState);
			}

			if (ConditionName == "LAUNCH_CODES_GIVEN")
			{
				PlayerData.LearnLaunchCodes();
			}
		}

		public override void OnReceiveLocal()
		{
			if (QSBCore.IsHost)
			{
				QSBWorldSync.SetDialogueCondition(ConditionName, ConditionState);
			}
		}
	}
}