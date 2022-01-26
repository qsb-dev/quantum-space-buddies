namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBCharacterAnimController : NpcAnimController<CharacterAnimController>
	{
		public override void SendResyncInfo(uint to)
		{
			// todo SendResyncInfo
		}

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._dialogueTree;

		public override bool InConversation()
			=> AttachedObject._inConversation;
	}
}
