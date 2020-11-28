using OWML.Common;
using OWML.ModHelper.Events;
using QSB.EventsCore;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.ConversationSync.Events
{
	public class ConversationStartEndEvent : QSBEvent<ConversationStartEndMessage>
	{
		public override EventsCore.EventType Type => EventsCore.EventType.ConversationStartEnd;

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

		public override void OnReceiveRemote(ConversationStartEndMessage message)
		{
			if (message.CharacterId == -1)
			{
				DebugLog.ToConsole("Warning - Received conv. start/end event with char id -1.", MessageType.Warning);
				return;
			}
			var dialogueTree = WorldRegistry.OldDialogueTrees[message.CharacterId];
			var animController = Resources.FindObjectsOfTypeAll<CharacterAnimController>().FirstOrDefault(x => x.GetValue<CharacterDialogueTree>("_dialogueTree") == dialogueTree);

			// Make character face player and talk
			if (animController != default(CharacterAnimController))
			{
				if (message.State)
				{
					// Start talking
					QSBPlayerManager.GetPlayer(message.PlayerId).CurrentDialogueID = message.CharacterId;
					animController.SetValue("_inConversation", true);
					animController.SetValue("_playerInHeadZone", true);
					if (animController.GetValue<bool>("_hasTalkAnimation"))
					{
						animController.GetValue<Animator>("_animator").SetTrigger("Talking");
					}
					dialogueTree.GetComponent<InteractVolume>().DisableInteraction();
				}
				else
				{
					// Stop talking
					QSBPlayerManager.GetPlayer(message.PlayerId).CurrentDialogueID = -1;
					animController.SetValue("_inConversation", false);
					animController.SetValue("_playerInHeadZone", false);
					if (animController.GetValue<bool>("_hasTalkAnimation"))
					{
						animController.GetValue<Animator>("_animator").SetTrigger("Idle");
					}
					dialogueTree.GetComponent<InteractVolume>().EnableInteraction();
				}
			}
		}
	}
}
