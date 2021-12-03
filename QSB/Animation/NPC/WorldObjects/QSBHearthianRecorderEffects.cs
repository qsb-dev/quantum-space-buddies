namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBHearthianRecorderEffects : NpcAnimController<HearthianRecorderEffects>
	{
		public override bool InConversation()
			=> AttachedObject._characterDialogueTree.InConversation();

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._characterDialogueTree;
	}
}
