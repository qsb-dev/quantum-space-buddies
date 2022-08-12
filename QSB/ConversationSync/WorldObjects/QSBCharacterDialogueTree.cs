using QSB.ConversationSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.ConversationSync.WorldObjects;

public class QSBCharacterDialogueTree : WorldObject<CharacterDialogueTree>
{
	public override void SendInitialState(uint to)
	{
		uint playeID = ConversationManager.Instance.GetPlayerTalkingToTree(AttachedObject);
		if (QSBPlayerManager.LocalPlayerId == playeID)//Only sends if the local player is the one talking to the tree
		{
			var key = AttachedObject._currentNode._name + AttachedObject._currentNode._listPagesToDisplay[AttachedObject._currentNode._currentPage]; this.SendMessage(new ConversationStartEndMessage(true) { To = to });//To tell the conversation has started
			this.SendMessage(new ConversationStartEndMessage(true) { To = to });//To tell the conversation has started
			new ConversationMessage(ConversationType.Character, ObjectId, key) { To = to }.Send();//To tell that there is a conversation bubble
			//new ConversationMessage(ConversationType.Player, ObjectId, key) { To = to }.Send();//To tell that there is a conversation bubble with option 
		}
	}
}
