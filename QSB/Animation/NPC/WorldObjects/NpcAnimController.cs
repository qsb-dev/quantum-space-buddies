using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Animation.NPC.WorldObjects
{
	internal abstract class NpcAnimController<T> : WorldObject<T>, INpcAnimController
		where T : MonoBehaviour
	{
		public abstract CharacterDialogueTree GetDialogueTree();

		public void StartConversation()
			=> GetDialogueTree().RaiseEvent(nameof(CharacterDialogueTree.OnStartConversation));

		public void EndConversation()
			=> GetDialogueTree().RaiseEvent(nameof(CharacterDialogueTree.OnEndConversation));
	}
}
