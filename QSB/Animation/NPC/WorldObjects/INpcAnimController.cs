using QSB.WorldSync;

namespace QSB.Animation.NPC.WorldObjects
{
	public interface INpcAnimController : IWorldObjectTypeSubset
	{
		CharacterDialogueTree GetDialogueTree();
		void StartConversation();
		void EndConversation();
		bool InConversation();
	}
}
