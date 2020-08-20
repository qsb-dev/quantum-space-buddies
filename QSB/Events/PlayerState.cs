using QSB.Messaging;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerState : NetworkBehaviour
    {
        public static PlayerState LocalInstance { get; private set; }

        private MessageHandler<PlayerStateMessage> _messageHandler;

        private void Awake()
        {
            _messageHandler = new MessageHandler<PlayerStateMessage>(MessageType.FullState);
            _messageHandler.OnClientReceiveMessage += OnClientReceiveMessage;

            LocalInstance = this;
        }

        private void OnClientReceiveMessage(PlayerStateMessage message)
        {
            if (message.AboutId == PlayerRegistry.LocalPlayerId)
            {
                return;
            }
            QSB.Helper.Events.Unity.RunWhen(() => PlayerRegistry.GetTransformSync(message.AboutId) != null,
                () => PlayerRegistry.HandleFullStateMessage(message));
        }

        public void Send()
        {
            foreach (var player in PlayerRegistry.PlayerList)
            {
                var message = new PlayerStateMessage
                {
                    AboutId = player.NetId,
                    PlayerName = player.Name,
                    PlayerReady = player.IsReady,
                    PlayerState = player.State
                };

                _messageHandler.SendToAll(message);
            }
        }
    }
}
