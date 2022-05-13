using QSB.WorldSync;

namespace QSB.Animation.NPC.WorldObjects;

internal class QSBCharacterAnimController : WorldObject<CharacterAnimController>
{
	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public CharacterDialogueTree GetDialogueTree()
		=> AttachedObject._dialogueTree;

	public bool InConversation()
		=> AttachedObject._inConversation;
}