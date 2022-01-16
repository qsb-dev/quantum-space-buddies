using Mirror;
using OWML.Common;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ConversationSync.Messages
{
	public class ConversationStartEndMessage : QSBBoolMessage
	{
		private int TreeId;
		private uint PlayerId;

		public ConversationStartEndMessage(int treeId, uint playerId, bool start)
		{
			TreeId = treeId;
			PlayerId = playerId;
			Value = start;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TreeId);
			writer.Write(PlayerId);
			writer.Write(Value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			TreeId = reader.Read<int>();
			PlayerId = reader.Read<uint>();
			Value = reader.Read<bool>();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

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
				StartConversation(PlayerId, TreeId, dialogueTree);
			}
			else
			{
				EndConversation(PlayerId, dialogueTree);
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