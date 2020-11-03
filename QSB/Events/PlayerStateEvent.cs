using QSB.Messaging;
using QSB.Player;
using QSB.Player.Events;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerStateEvent : NetworkBehaviour
    {
        public static PlayerStateEvent LocalInstance { get; private set; }

        private MessageHandler<PlayerStateMessage> _messageHandler;

        private void Awake()
        {
            _messageHandler = new MessageHandler<PlayerStateMessage>(EventType.FullState);
            _messageHandler.OnClientReceiveMessage += OnClientReceiveMessage;

            LocalInstance = this;
        }

        private void OnClientReceiveMessage(PlayerStateMessage message)
        {
            if (message.AboutId == QSBPlayerManager.LocalPlayerId)
            {
                return;
            }
            QSB.Helper.Events.Unity.RunWhen(
                () => QSBPlayerManager.GetSyncObject<TransformSync.TransformSync>(message.AboutId) != null,
                () => QSBPlayerManager.HandleFullStateMessage(message));
        }

        public void Send()
        {
            foreach (var player in QSBPlayerManager.PlayerList)
            {
                var message = new PlayerStateMessage
                {
                    AboutId = player.PlayerId,
                    PlayerName = player.Name,
                    PlayerReady = player.IsReady,
                    PlayerState = player.State
                };

                _messageHandler.SendToAll(message);
            }
        }
    }
}
