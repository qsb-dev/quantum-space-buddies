using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.ConversationSync
{
    class ConversationEvent : QSBEvent<ConversationMessage>
    {
        public override EventType Type => EventType.Conversation;

        public override void SetupListener() => GlobalMessenger<int, string, ConversationType>.AddListener(EventNames.QSBConversation, Handler);

        public override void CloseListener() => GlobalMessenger<int, string, ConversationType>.RemoveListener(EventNames.QSBConversation, Handler);

        private void Handler(int id, string message, ConversationType type) => SendEvent(CreateMessage(id, message, type));

        private ConversationMessage CreateMessage(int id, string message, ConversationType type) => new ConversationMessage
        {
            AboutId = LocalPlayerId,
            ObjectId = id,
            Type = type,
            Message = message
        };

        public override void OnReceiveRemote(ConversationMessage message)
        {
            DebugLog.DebugWrite($"Got conversation event for type [{message.Type}] id [{message.ObjectId}] text [{message.Message}]");
        }
    }
}
