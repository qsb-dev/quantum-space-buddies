using OWML.Common;
using QSB.ConversationSync.WorldObjects;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages;

public class ConversationStartEndMessage : QSBMessage<(int TreeId, bool Start)>
{
	public ConversationStartEndMessage(int treeId, bool start) : base((treeId, start)) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var dialogueTree = Data.TreeId.GetWorldObject<QSBCharacterDialogueTree>();

		if (Data.Start)
		{
			StartConversation(From, dialogueTree);
		}
		else
		{
			EndConversation(From, dialogueTree);
		}
	}

	private static void StartConversation(
		uint playerId,
		QSBCharacterDialogueTree tree)
	{
		QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTree = tree;
		tree.AttachedObject.GetInteractVolume().DisableInteraction();
		tree.AttachedObject.RaiseEvent(nameof(CharacterDialogueTree.OnStartConversation));
	}

	private static void EndConversation(
		uint playerId,
		QSBCharacterDialogueTree tree)
	{
		QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTree = null;
		tree.AttachedObject.GetInteractVolume().EnableInteraction();
		tree.AttachedObject.RaiseEvent(nameof(CharacterDialogueTree.OnEndConversation));
	}
}