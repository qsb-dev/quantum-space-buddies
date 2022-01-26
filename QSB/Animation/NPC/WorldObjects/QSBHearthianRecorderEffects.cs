namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBHearthianRecorderEffects : NpcAnimController<HearthianRecorderEffects>
	{
		public override void SendResyncInfo(uint to)
		{
			// todo SendResyncInfo
		}

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._characterDialogueTree;
	}
}
