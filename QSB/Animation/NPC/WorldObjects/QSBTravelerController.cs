using OWML.Utils;
using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Animation.NPC.WorldObjects
{
	class QSBTravelerController : NpcAnimController<TravelerController>
	{
		public override CharacterDialogueTree GetDialogueTree()
			=> AttachedObject.GetValue<CharacterDialogueTree>("_dialogueSystem");

		public override bool InConversation()
			=> AttachedObject.GetValue<bool>("_talking");
	}
}
