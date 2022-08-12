using QSB.ConversationSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QSB.ConversationSync.WorldObjects;

public class QSBCharacterDialogueTree : WorldObject<CharacterDialogueTree>
{
	public override void SendInitialState(uint to) 
	{
		if (AttachedObject.enabled && AttachedObject._currentNode != null)//Only sends if there is a conversation happening
		{
			var key = AttachedObject._currentNode._name + AttachedObject._currentNode._listPagesToDisplay[AttachedObject._currentNode._currentPage];
			this.SendMessage(new CharacterDialogueTreeMessage(key, ConversationManager.Instance.GetPlayerTalkingToTree(AttachedObject)) { To = to });
		}
	}

	public void SetInConversation(string currentText, uint playerId) 
	{
        QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTree = this;
        AttachedObject.GetInteractVolume().DisableInteraction();
		AttachedObject.RaiseEvent(nameof(CharacterDialogueTree.OnStartConversation));

		var translated = TextTranslation.Translate(currentText).Trim();
		translated = Regex.Replace(translated, @"<[Pp]ause=?\d*\.?\d*\s?\/?>", "");
		ConversationManager.Instance.DisplayCharacterConversationBox(ObjectId, translated);
	}
}
