using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class GameState : NetworkBehaviour
    {
        public static GameState LocalInstance { get; private set; }

        private MessageHandler<FullStateMessage> _messageHandler;

        private void Awake()
        {
            _messageHandler = new MessageHandler<FullStateMessage>(MessageType.FullState);
            _messageHandler.OnClientReceiveMessage += OnClientReceiveMessage;

            LocalInstance = this;
        }

        private void OnClientReceiveMessage(FullStateMessage message)
        {
            if (message.SenderId == PlayerTransformSync.LocalInstance.netId.Value)
            {
                return;
            }
            DebugLog.ToConsole($"Received game state for id {message.SenderId}");
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
                DebugLog.ToConsole($"* Sent state for {player.NetId}");
            }
        }
    }
}
