using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages
{
	public class ConversationStartEndMessage : QSBMessage<bool, int>
	{
		public ConversationStartEndMessage(int treeId, bool start)
		{
			Value2 = treeId;
			Value1 = start;
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			if (Value2 == -1)
			{
				DebugLog.ToConsole("Warning - Received conv. start/end event with char id -1.", MessageType.Warning);
				return;
			}

			var dialogueTree = QSBWorldSync.OldDialogueTrees[Value2];

			if (Value1)
			{
				StartConversation(From, Value2, dialogueTree);
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
}