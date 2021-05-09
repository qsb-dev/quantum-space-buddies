namespace QSB.Animation.NPC.WorldObjects
{
	public interface INpcAnimController
	{
		CharacterDialogueTree GetDialogueTree();
		void StartConversation();
		void EndConversation();
		bool InConversation();
	}
}
