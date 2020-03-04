using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerJoin : NetworkBehaviour
    {
        private MessageHandler<JoinEvent> _joinHandler;

        private void Awake()
        {
            _joinHandler = new MessageHandler<JoinEvent>();
            _joinHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _joinHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        public void Join(string playerName)
        {
            var message = new JoinEvent
            {
                PlayerName = playerName
            };
            if (isServer)
            {
                _joinHandler.SendToAll(message);
            }
            else
            {
                _joinHandler.SendToServer(message);
            }
        }
        
        private void OnServerReceiveMessage(JoinEvent message)
        {
            _joinHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(JoinEvent message)
        {
            DebugLog.All(message.PlayerName, "joined!");
        }

    }
}
