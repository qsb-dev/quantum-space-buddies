using QSB.EventsCore;
using QSB.Utility;

namespace QSB.DialogueConditionSync
{
	public class DialogueConditionEvent : QSBEvent<DialogueConditionMessage>
	{
		public override EventType Type => EventType.DialogueCondition;

		public override void SetupListener() => GlobalMessenger<string, bool>.AddListener(EventNames.DialogueCondition, Handler);

		public override void CloseListener() => GlobalMessenger<string, bool>.RemoveListener(EventNames.DialogueCondition, Handler);

		private void Handler(string name, bool state) => SendEvent(CreateMessage(name, state));

		private DialogueConditionMessage CreateMessage(string name, bool state) => new DialogueConditionMessage
		{
			AboutId = LocalPlayerId,
			ConditionName = name,
			ConditionState = state
		};

		public override void OnReceiveRemote(DialogueConditionMessage message)
		{
			DebugLog.DebugWrite($"dialoguecondition \"{message.ConditionName} to {message.ConditionState}\"");
			DialogueConditionManager.SharedInstance.SetConditionState(message.ConditionName, message.ConditionState);
		}
	}
}