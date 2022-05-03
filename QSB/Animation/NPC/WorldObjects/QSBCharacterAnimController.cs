namespace QSB.Animation.NPC.WorldObjects;

internal class QSBCharacterAnimController : NpcAnimController<CharacterAnimController>
{
	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public override CharacterDialogueTree GetDialogueTree()
		=> AttachedObject._dialogueTree;

	public bool InConversation()
		=> AttachedObject._inConversation;
}