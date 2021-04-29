using OWML.Common;
using OWML.Utils;
using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.ConversationSync.Events
{
	public class ConversationStartEndEvent : QSBEvent<ConversationStartEndMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.ConversationStartEnd;

		public override void SetupListener() => GlobalMessenger<int, uint, bool>.AddListener(EventNames.QSBConversationStartEnd, Handler);
		public override void CloseListener() => GlobalMessenger<int, uint, bool>.RemoveListener(EventNames.QSBConversationStartEnd, Handler);

		private void Handler(int objId, uint playerId, bool state) => SendEvent(CreateMessage(objId, playerId, state));

		private ConversationStartEndMessage CreateMessage(int objId, uint playerId, bool state) => new ConversationStartEndMessage
		{
			AboutId = LocalPlayerId,
			TreeId = objId,
			PlayerId = playerId,
			State = state
		};

		public override void OnReceiveRemote(bool server, ConversationStartEndMessage message)
		{
			DebugLog.DebugWrite($"Get conversation event charId:{message.TreeId}, state:{message.State}");
			if (message.TreeId == -1)
			{
				DebugLog.ToConsole("Warning - Received conv. start/end event with char id -1.", MessageType.Warning);
				return;
			}

			if (!QSBCore.HasWokenUp)
			{
				return;
			}

			var dialogueTree = QSBWorldSync.OldDialogueTrees[message.TreeId];
			var animController = Resources.FindObjectsOfTypeAll<CharacterAnimController>().FirstOrDefault(x => x.GetValue<CharacterDialogueTree>("_dialogueTree") == dialogueTree);

			if (animController == default)
			{
				return;
			}

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
			DebugLog.DebugWrite($"Set player {playerId} to treeId {dialogueTreeId}");
			QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTreeId = dialogueTreeId;
			tree.GetInteractVolume().DisableInteraction();
		}

		private void EndConversation(
			uint playerId,
			CharacterDialogueTree tree)
		{
			DebugLog.DebugWrite($"Set player {playerId} to treeId -1");
			QSBPlayerManager.GetPlayer(playerId).CurrentCharacterDialogueTreeId = -1;
			tree.GetInteractVolume().EnableInteraction();
		}
	}
}