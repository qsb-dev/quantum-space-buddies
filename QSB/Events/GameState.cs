using System.Collections;
using System.Collections.Generic;
using QSB.Messaging;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    class GameState : NetworkBehaviour
    {
        private MessageHandler<FullStateMessage> _messageHandler;

        private void Awake()
        {
            _messageHandler = new MessageHandler<FullStateMessage>();
            _messageHandler.OnClientReceiveMessage += OnClientReceiveMessage;
        }

        private void OnClientReceiveMessage(FullStateMessage message)
        {
            PlayerJoin.PlayerNames = message.PlayerNames;
        }

        public void Send()
        {
            var message = new FullStateMessage()
            {
                PlayerNames = PlayerJoin.PlayerNames
            };

            _messageHandler.SendToAll(message);
        }
    }
}
