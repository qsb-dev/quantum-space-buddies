using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Animation.NPC.WorldObjects
{
	public interface INpcAnimController
	{
		CharacterDialogueTree GetDialogueTree();
		void StartConversation();
		void EndConversation();
	}
}
