using QSB.Messaging;
using QSB.Player.TransformSync;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.ConversationSync.Messages
{
	public class DialogueConditionMessage : QSBMessage
	{
		static DialogueConditionMessage() => GlobalMessenger<string, bool>.AddListener(OWEvents.DialogueConditionChanged, Handler);

		private static void Handler(string name, bool state)
		{
			if (PlayerTransformSync.LocalInstance)
			{
				new DialogueConditionMessage(name, state).Send();
			}
		}

		private string ConditionName;
		private bool ConditionState;

		public DialogueConditionMessage(string name, bool state)
		{
			ConditionName = name;
			ConditionState = state;
		}

		public DialogueConditionMessage() { }

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

			DialogueConditionManager.SharedInstance.SetConditionState(ConditionName, ConditionState);
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