using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Swag
{
    public class Hello : NetworkBehaviour
    {
        private MessageHandler<HelloMessage> _helloHandler;

        private void Awake()
        {
            _helloHandler = new MessageHandler<HelloMessage>();
            _helloHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _helloHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        public void Sup(string playerName)
        {
            var message = new HelloMessage
            {
                PlayerName = playerName
            };
            if (isServer)
            {
                _helloHandler.SendToAll(message);
            }
            else
            {
                _helloHandler.SendToServer(message);
            }
        }
        
        private void OnServerReceiveMessage(HelloMessage message)
        {
            _helloHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(HelloMessage message)
        {
            DebugLog.Screen(message.PlayerName + " joined!");
        }

    }
}
