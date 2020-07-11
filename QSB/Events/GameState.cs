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
        public static GameState LocalInstance { get; private set; }

        private MessageHandler<FullStateMessage> _messageHandler;

        private void Awake()
        {
            _messageHandler = new MessageHandler<FullStateMessage>();
            _messageHandler.OnClientReceiveMessage += OnClientReceiveMessage;

            LocalInstance = this;
        }

        private void OnClientReceiveMessage(FullStateMessage message)
        {
            Finder.UpdatePlayerNames(message.PlayerNames);
        }

        public void Send()
        {
            var message = new FullStateMessage()
            {
                PlayerNames = Finder.GetPlayerNames()
            };

            _messageHandler.SendToAll(message);
        }
    }
}
