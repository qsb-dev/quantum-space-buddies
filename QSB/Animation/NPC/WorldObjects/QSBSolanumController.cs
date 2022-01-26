namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBSolanumController : NpcAnimController<NomaiConversationManager>
	{
		public override void SendInitialState(uint to)
		{
			// todo SendResyncInfo
		}

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._characterDialogueTree;
	}
}
