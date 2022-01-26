namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBSolanumController : NpcAnimController<NomaiConversationManager>
	{
		public override void SendResyncInfo(uint to)
		{
			// todo
		}

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._characterDialogueTree;
	}
}
