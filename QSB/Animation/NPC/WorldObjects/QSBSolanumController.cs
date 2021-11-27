namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBSolanumController : NpcAnimController<NomaiConversationManager>
	{
		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._characterDialogueTree;

		public override bool InConversation()
			=> AttachedObject._solanumAnimController._animator.GetBool("ListeningToPlayer");
	}
}
