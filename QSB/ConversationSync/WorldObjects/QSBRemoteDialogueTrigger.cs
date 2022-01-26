using QSB.WorldSync;

namespace QSB.ConversationSync.WorldObjects
{
	internal class QSBRemoteDialogueTrigger : WorldObject<RemoteDialogueTrigger>
	{
		public override void SendResyncInfo(uint to)
		{
			// todo SendResyncInfo
		}

		public void RemoteEnterDialogue(int dialogueIndex)
		{
			var dialogueCondition = AttachedObject._listDialogues[dialogueIndex];
			AttachedObject._activeRemoteDialogue = dialogueCondition.dialogue;
			AttachedObject._inRemoteDialogue = true;

			AttachedObject._activatedDialogues[dialogueIndex] = true;
		}
	}
}
