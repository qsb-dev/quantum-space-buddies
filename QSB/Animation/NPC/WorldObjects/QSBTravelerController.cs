using OWML.Utils;

namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBTravelerController : NpcAnimController<TravelerController>
	{
		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._dialogueSystem;

		public override bool InConversation()
			=> AttachedObject._talking;
	}
}
