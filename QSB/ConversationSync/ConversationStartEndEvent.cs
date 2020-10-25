using OWML.Common;
using OWML.ModHelper.Events;
using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.ConversationSync
{
    public class ConversationStartEndEvent : QSBEvent<ConversationStartEndMessage>
    {
        public override Messaging.EventType Type => Messaging.EventType.ConversationStartEnd;

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
                    PlayerRegistry.GetPlayer(message.PlayerId).CurrentDialogueID = message.CharacterId;
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
                    PlayerRegistry.GetPlayer(message.PlayerId).CurrentDialogueID = -1;
                    animController.SetValue("_inConversation", false);
                    animController.SetValue("_playerInHeadZone", false);
                    if (animController.GetValue<bool>("_hasTalkAnimation"))
                    {
                        animController.GetValue<Animator>("_animator").SetTrigger("Idle");
                    }
                    dialogueTree.GetComponent<InteractVolume>().EnableInteraction();
                }
            }

            // Make character turn to player (if they're meant to)
            var qsbFacePlayer = dialogueTree.GetComponentInParent<QSBFacePlayerWhenTalking>();
            if (qsbFacePlayer != null)
            {
                if (message.State)
                {
                    DebugLog.DebugWrite("start convo faceplayer for " + message.CharacterId);
                    qsbFacePlayer.StartConversation(PlayerRegistry.GetPlayer(message.PlayerId).Body.transform.position);
                }
                else
                {
                    qsbFacePlayer.EndConversation();
                }
            }
        }
    }
}
