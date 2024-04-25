using OWML.Utils;
using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.ConversationSync.Messages;

public class ConversationStartEndMessage : QSBWorldObjectMessage<QSBCharacterDialogueTree, (uint playerId, bool start)>
{
	public ConversationStartEndMessage(uint playerId, bool start) : base((playerId, start)) { }

	public override void OnReceiveRemote()
	{
		if (Data.start)
		{
			QSBPlayerManager.GetPlayer(Data.playerId).CurrentCharacterDialogueTree = WorldObject;
			WorldObject.AttachedObject.GetInteractVolume().DisableInteraction();
			WorldObject.AttachedObject.RaiseEvent(nameof(CharacterDialogueTree.OnStartConversation));
		}
		else
		{
			QSBPlayerManager.GetPlayer(Data.playerId).CurrentCharacterDialogueTree = null;
			WorldObject.AttachedObject.GetInteractVolume().EnableInteraction();
			WorldObject.AttachedObject.RaiseEvent(nameof(CharacterDialogueTree.OnEndConversation));
		}
	}
}
