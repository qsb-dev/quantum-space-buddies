using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages;

public class ConversationStartEndMessage : QSBMessage<bool>
{
	private int TreeId;

	public ConversationStartEndMessage(int treeId, bool start)
	{
		TreeId = treeId;
		Value = start;
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(TreeId);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		TreeId = reader.Read<int>();
	}

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		if (TreeId == -1)
		{
			DebugLog.ToConsole("Warning - Received conv. start/end event with char id -1.", MessageType.Warning);
			return;
		}

		var dialogueTree = QSBWorldSync.OldDialogueTrees[TreeId];

		if (Value)
		{
			StartConversation(From, TreeId, dialogueTree);
		}
		else
		{
			EndConversation(From, dialogueTree);
		}
	}

	private static void StartConversation(
		uint playerId,
		int treeId,
		CharacterDialogueTree tree)
	{
		QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTreeId = treeId;
		tree.GetInteractVolume().DisableInteraction();
	}

	private static void EndConversation(
		uint playerId,
		CharacterDialogueTree tree)
	{
		QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTreeId = -1;
		tree.GetInteractVolume().EnableInteraction();
	}
}