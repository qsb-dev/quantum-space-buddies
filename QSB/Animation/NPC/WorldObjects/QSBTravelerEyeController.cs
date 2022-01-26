namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBTravelerEyeController : NpcAnimController<TravelerEyeController>
	{
		public override void SendResyncInfo(uint to)
		{
			// todo SendResyncInfo
		}

		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject._dialogueTree;
	}
}
