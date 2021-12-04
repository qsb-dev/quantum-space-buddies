using QSB.Events;
using QSB.WorldSync;

namespace QSB.ConversationSync.Events
{
	public class DialogueConditionEvent : QSBEvent<DialogueConditionMessage>
	{
		public override void SetupListener() => GlobalMessenger<string, bool>.AddListener(EventNames.DialogueConditionChanged, Handler);
		public override void CloseListener() => GlobalMessenger<string, bool>.RemoveListener(EventNames.DialogueConditionChanged, Handler);

		private void Handler(string name, bool state) => SendEvent(CreateMessage(name, state));

		private DialogueConditionMessage CreateMessage(string name, bool state) => new()
		{
			AboutId = LocalPlayerId,
			ConditionName = name,
			ConditionState = state
		};

		public override void OnReceiveLocal(bool server, DialogueConditionMessage message)
		{
			if (server)
			{
				QSBWorldSync.SetDialogueCondition(message.ConditionName, message.ConditionState);
			}
		}

		public override void OnReceiveRemote(bool server, DialogueConditionMessage message)
		{
			if (server)
			{
				QSBWorldSync.SetDialogueCondition(message.ConditionName, message.ConditionState);
			}

			DialogueConditionManager.SharedInstance.SetConditionState(message.ConditionName, message.ConditionState);
		}
	}
}