using QSB.ConversationSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.ConversationSync.WorldObjects;

/// <summary>
/// BUG: do conversation leave on player leave so other people can actually talk lol
/// </summary>
public class QSBCharacterDialogueTree : WorldObject<CharacterDialogueTree>
{
	public override void SendInitialState(uint to)
	{
		var playerId = ConversationManager.Instance.GetPlayerTalkingToTree(AttachedObject);
		if (playerId != uint.MaxValue)
		{
			this.SendMessage(new ConversationStartEndMessage(playerId, true) { To = to });
		}
		// TODO: maybe also sync the dialogue box and player box?
	}
}
