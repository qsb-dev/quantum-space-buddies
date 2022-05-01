namespace QSB.Animation.NPC.WorldObjects;

internal class PrisonerDialogue : NpcAnimController<PrisonerDirector>
{
	public override void SendInitialState(uint to)
	{
		// todo : implement
	}

	public override CharacterDialogueTree GetDialogueTree()
		=> AttachedObject._characterDialogueTree;
}
