using System.Collections;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class FullStateRequest : NetworkBehaviour
    {
        public static FullStateRequest Instance { get; private set; }

        private MessageHandler<StateRequestMessage> _stateRequestHandler;

        private void Awake()
        {
            Instance = this;

            _stateRequestHandler = new MessageHandler<StateRequestMessage>(MessageType.FullStateRequest);
            _stateRequestHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        public void Request()
        {
            DebugLog.ToConsole("Requesting gamestate...");
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
            DebugLog.ToConsole("Received request for gamestate.");
            GameState.LocalInstance.Send();
        }
    }
}
