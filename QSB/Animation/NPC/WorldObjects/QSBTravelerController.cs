namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBTravelerController : NpcAnimController<TravelerController>
	{
		public override void SendResyncInfo(uint to)
		{
			// todo
		}

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._dialogueSystem;
	}
}
