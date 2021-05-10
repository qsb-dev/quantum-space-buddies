using OWML.Utils;

namespace QSB.Animation.NPC.WorldObjects
{
	internal class QSBTravelerController : NpcAnimController<TravelerController>
	{
		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject.GetValue<CharacterDialogueTree>("_dialogueSystem");

		public override bool InConversation()
			=> AttachedObject.GetValue<bool>("_talking");
	}
}
