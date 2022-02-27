using QSB.WorldSync;

namespace QSB.Animation.NPC.WorldObjects
{
	public interface INpcAnimController : IWorldObject
	{
		CharacterDialogueTree GetDialogueTree();
		void StartConversation();
		void EndConversation();
	}
}