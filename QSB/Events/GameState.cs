using QSB.Messaging;
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
            DebugLog.ToConsole($"Received game state id {PlayerRegistry.LocalPlayer.NetId}");
            PlayerRegistry.HandleFullStateMessage(message);
        }

        public void Send()
        {
            DebugLog.ToConsole("Sending game state to all players.");
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
