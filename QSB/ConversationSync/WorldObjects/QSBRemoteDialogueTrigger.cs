using QSB.WorldSync;

namespace QSB.ConversationSync.WorldObjects
{
	internal class QSBRemoteDialogueTrigger : WorldObject<RemoteDialogueTrigger>
	{
		public void RemoteEnterDialogue(int activatedIndex, int listIndex)
		{
			var dialogueCondition = AttachedObject._listDialogues[listIndex];
			AttachedObject._activeRemoteDialogue = dialogueCondition.dialogue;
			AttachedObject._inRemoteDialogue = true;
			AttachedObject._activatedDialogues[activatedIndex] = true;
		}
	}
}
