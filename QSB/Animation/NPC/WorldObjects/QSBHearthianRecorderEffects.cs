namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBHearthianRecorderEffects : NpcAnimController<HearthianRecorderEffects>
	{
		public override void SendInitialState(uint to)
		{
			// todo SendResyncInfo
		}

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._characterDialogueTree;
	}
}
