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
            _messageHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        private void OnServerReceiveMessage(FullStateMessage message)
        {
            _messageHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(FullStateMessage message)
        {
            PlayerJoin.PlayerNames = message.PlayerNames;
        }
    }
}
