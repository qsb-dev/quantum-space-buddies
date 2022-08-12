using QSB.ConversationSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.ConversationSync.WorldObjects;

public class QSBCharacterDialogueTree : WorldObject<CharacterDialogueTree>
{
	public override void SendInitialState(uint to)
	{
		if (AttachedObject.enabled && AttachedObject._currentNode != null)//Only sends if there is a conversation happening
		{
			var key = AttachedObject._currentNode._name + AttachedObject._currentNode._listPagesToDisplay[AttachedObject._currentNode._currentPage]; this.SendMessage(new ConversationStartEndMessage(true) { To = to });//To tell the conversation has started
			new ConversationMessage(ConversationType.Character, ObjectId, key) { To = to }.Send();//To tell that there is a conversation bubble
			this.SendMessage(new ConversationStartEndMessage(true) { To = to });//To tell the conversation has started
			new ConversationMessage(ConversationType.Character, ObjectId, key) { To = to }.Send();//To tell that there is a conversation bubble
		}
	}
}
