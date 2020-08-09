using System.Collections;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerStatesRequest : NetworkBehaviour
    {
        public static PlayerStatesRequest Instance { get; private set; }

        private MessageHandler<PlayerStatesRequestMessage> _stateRequestHandler;

        private void Awake()
        {
            Instance = this;

            _stateRequestHandler = new MessageHandler<PlayerStatesRequestMessage>(MessageType.FullStateRequest);
            _stateRequestHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        public void Request()
        {
            StartCoroutine(SendRequest());
        }

        private IEnumerator SendRequest()
        {
            yield return new WaitUntil(() => PlayerTransformSync.LocalInstance != null);
            var message = new PlayerStatesRequestMessage
            {
                SenderId = PlayerTransformSync.LocalInstance.netId.Value
            };
            _stateRequestHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(PlayerStatesRequestMessage message)
        {
            PlayerState.LocalInstance.Send();
        }
    }
}
