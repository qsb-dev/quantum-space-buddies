namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBHearthianRecorderEffects : NpcAnimController<HearthianRecorderEffects>
	{
		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._characterDialogueTree;
	}
}
