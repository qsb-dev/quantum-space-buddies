using OWML.Common;
using OWML.ModHelper.Events;
using QSB.Events;
using QSB.Messaging;
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
                return;
            }
            var dialogueTree = WorldRegistry.OldDialogueTrees[message.CharacterId];
            var animController = Resources.FindObjectsOfTypeAll<CharacterAnimController>().FirstOrDefault(x => x.GetValue<CharacterDialogueTree>("_dialogueTree") == dialogueTree);
            if (animController != default(CharacterAnimController))
            {
                if (message.State)
                {
                    PlayerRegistry.GetPlayer(message.PlayerId).CurrentDialogueID = message.CharacterId;
                    animController.SetValue("_inConversation", true);
                    animController.SetValue("_playerInHeadZone", true);
                    if (animController.GetValue<bool>("_hasTalkAnimation"))
                    {
                        animController.GetValue<Animator>("_animator").SetTrigger("Talking");
                    }
                }
                else
                {
                    PlayerRegistry.GetPlayer(message.PlayerId).CurrentDialogueID = -1;
                    animController.SetValue("_inConversation", false);
                    animController.SetValue("_playerInHeadZone", false);
                    if (animController.GetValue<bool>("_hasTalkAnimation"))
                    {
                        animController.GetValue<Animator>("_animator").SetTrigger("Idle");
                    }
                }
            }
            
            /*
            var qsbFacePlayer = dialogueTree.GetComponent<QSBFacePlayerWhenTalking>();
            if (qsbFacePlayer == null)
            {
                DebugLog.ToConsole($"Error - QSBFacePlayerWhenTalking not found for object ID {message.CharacterId}!", MessageType.Error);
            }
            if (message.State)
            {
                qsbFacePlayer.StartConversation(PlayerRegistry.GetPlayer(message.PlayerId).Camera.transform.position);
            }
            else
            {
                qsbFacePlayer.EndConversation();
            }
            */
        }
    }
}
