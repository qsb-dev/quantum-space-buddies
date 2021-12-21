namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBTravelerEyeController : NpcAnimController<TravelerEyeController>
	{
		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._dialogueTree;
	}
}
