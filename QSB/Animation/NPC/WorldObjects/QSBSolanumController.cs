namespace QSB.Animation.NPC.WorldObjects;

internal class QSBSolanumController : NpcAnimController<NomaiConversationManager>
{
	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public override CharacterDialogueTree GetDialogueTree()
		=> AttachedObject._characterDialogueTree;
}