using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class GameState : NetworkBehaviour
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
            PlayerRegistry.HandleFullStateMessage(message);
        }

        public void Send()
        {
            foreach (var player in PlayerRegistry.PlayerList)
            {
                var message = new FullStateMessage
                {
                    PlayerName = player.Name,
                    SenderId = player.NetId
                };

                _messageHandler.SendToAll(message);
            }
        }
    }
}
