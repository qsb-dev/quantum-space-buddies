using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.ConversationSync.Messages;

public class ConversationStartEndMessage : QSBWorldObjectMessage<QSBCharacterDialogueTree, bool>
{
	public ConversationStartEndMessage(bool start) : base(start) { }

	public override void OnReceiveRemote()
	{
		if (Data)
		{
			QSBPlayerManager.GetPlayer(From).CurrentCharacterDialogueTree = WorldObject;
			WorldObject.AttachedObject.GetInteractVolume().DisableInteraction();
			WorldObject.AttachedObject.RaiseEvent(nameof(CharacterDialogueTree.OnStartConversation));
		}
		else
		{
			QSBPlayerManager.GetPlayer(From).CurrentCharacterDialogueTree = null;
			WorldObject.AttachedObject.GetInteractVolume().EnableInteraction();
			WorldObject.AttachedObject.RaiseEvent(nameof(CharacterDialogueTree.OnEndConversation));
		}
	}
}
