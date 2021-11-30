using OWML.Common;
using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ConversationSync.Events
{
	public class ConversationStartEndEvent : QSBEvent<ConversationStartEndMessage>
	{
		public override void SetupListener() => GlobalMessenger<int, uint, bool>.AddListener(EventNames.QSBConversationStartEnd, Handler);
		public override void CloseListener() => GlobalMessenger<int, uint, bool>.RemoveListener(EventNames.QSBConversationStartEnd, Handler);

		private void Handler(int objId, uint playerId, bool state) => SendEvent(CreateMessage(objId, playerId, state));

		private ConversationStartEndMessage CreateMessage(int objId, uint playerId, bool state) => new()
		{
			AboutId = LocalPlayerId,
			TreeId = objId,
			PlayerId = playerId,
			State = state
		};

		public override void OnReceiveRemote(bool server, ConversationStartEndMessage message)
		{
			if (message.TreeId == -1)
			{
				DebugLog.ToConsole("Warning - Received conv. start/end event with char id -1.", MessageType.Warning);
				return;
			}

			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var dialogueTree = QSBWorldSync.OldDialogueTrees[message.TreeId];

			if (message.State)
			{
				StartConversation(message.PlayerId, message.TreeId, dialogueTree);
			}
			else
			{
				EndConversation(message.PlayerId, dialogueTree);
			}
		}

		private void StartConversation(
			uint playerId,
			int dialogueTreeId,
			CharacterDialogueTree tree)
		{
			QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTreeId = dialogueTreeId;
			tree.GetInteractVolume().DisableInteraction();
		}

		private void EndConversation(
			uint playerId,
			CharacterDialogueTree tree)
		{
			QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTreeId = -1;
			tree.GetInteractVolume().EnableInteraction();
		}
	}
}