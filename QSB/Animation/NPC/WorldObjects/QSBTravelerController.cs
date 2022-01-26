namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBTravelerController : NpcAnimController<TravelerController>
	{
		public override void SendInitialState(uint to)
		{
			// todo SendResyncInfo
		}

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._dialogueSystem;
	}
}
