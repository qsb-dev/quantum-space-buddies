using OWML.Common;
using OWML.Utils;
using QSB.Animation.Character.WorldObjects;
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

		private void Handler(int charId, uint playerId, bool state) => SendEvent(CreateMessage(charId, playerId, state));

		private ConversationStartEndMessage CreateMessage(int charId, uint playerId, bool state) => new ConversationStartEndMessage
		{
			AboutId = LocalPlayerId,
			CharacterId = charId,
			PlayerId = playerId,
			State = state
		};

		public override void OnReceiveRemote(bool server, ConversationStartEndMessage message)
		{
			if (message.CharacterId == -1)
			{
				DebugLog.ToConsole("Warning - Received conv. start/end event with char id -1.", MessageType.Warning);
				return;
			}

			if (!QSBCore.HasWokenUp)
			{
				return;
			}

			var dialogueTree = QSBWorldSync.OldDialogueTrees[message.CharacterId];
			var animController = Resources.FindObjectsOfTypeAll<CharacterAnimController>().FirstOrDefault(x => x.GetValue<CharacterDialogueTree>("_dialogueTree") == dialogueTree);

			if (animController == default(CharacterAnimController))
			{
				return;
			}

			if (message.State)
			{
				StartConversation(message.PlayerId, message.CharacterId, animController, dialogueTree);
			}
			else
			{
				EndConversation(message.PlayerId, animController, dialogueTree);
			}
		}

		private void StartConversation(
			uint playerId,
			int characterId,
			CharacterAnimController controller,
			CharacterDialogueTree tree)
		{
			QSBPlayerManager.GetPlayer(playerId).CurrentDialogueID = characterId;
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCharacterAnimController, CharacterAnimController>(controller);
			qsbObj.StartConversation();
			tree.GetInteractVolume().DisableInteraction();
		}

		private void EndConversation(
			uint playerId,
			CharacterAnimController controller,
			CharacterDialogueTree tree)
		{
			QSBPlayerManager.GetPlayer(playerId).CurrentDialogueID = -1;
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCharacterAnimController, CharacterAnimController>(controller);
			qsbObj.EndConversation();
			tree.GetInteractVolume().EnableInteraction();
		}
	}
}