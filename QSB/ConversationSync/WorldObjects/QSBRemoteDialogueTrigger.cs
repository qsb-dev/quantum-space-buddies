using QSB.ConversationSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ConversationSync.WorldObjects;

public class QSBRemoteDialogueTrigger : WorldObject<RemoteDialogueTrigger>
{
	public override void SendInitialState(uint to) =>
		this.SendMessage(new RemoteDialogueInitialStateMessage(AttachedObject) { To = to });

	public void RemoteEnterDialogue(int dialogueIndex)
	{
		var dialogueCondition = AttachedObject._listDialogues[dialogueIndex];
		AttachedObject._activeRemoteDialogue = dialogueCondition.dialogue;
		AttachedObject._inRemoteDialogue = true;

		AttachedObject._activatedDialogues[dialogueIndex] = true;
	}
}