using System.Collections;
using System.Collections.Generic;
using QSB.Messaging;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class FullStateRequest : NetworkBehaviour
    {
        public static FullStateRequest LocalInstance { get; private set; }

        private MessageHandler<StateRequestMessage> _stateRequestHandler;

        private void Awake()
        {
            _stateRequestHandler = new MessageHandler<StateRequestMessage>();
            _stateRequestHandler.OnServerReceiveMessage += OnServerReceiveMessage;

            LocalInstance = this;
        }

        public void Request()
        {
            StartCoroutine(SendRequest());
        }

        private IEnumerator SendRequest()
        {
            yield return new WaitUntil(() => PlayerTransformSync.LocalInstance != null);
            var message = new StateRequestMessage
            {
                SenderId = PlayerTransformSync.LocalInstance.netId.Value
            };
            _stateRequestHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(StateRequestMessage message)
        {
            GameState.LocalInstance.Send();
        }
    }
}
