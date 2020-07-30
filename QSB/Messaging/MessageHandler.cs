using System;
using UnityEngine.Networking;

namespace QSB.Messaging
{
    // Extend this to create new message handlers.
    public class MessageHandler<T> where T : QSBMessage, new()
    {
        public event Action<T> OnClientReceiveMessage;
        public event Action<T> OnServerReceiveMessage;

        public MessageHandler()
        {
            if (QSBNetworkManager.IsReady)
            {
                Init();
            }
            else
            {
                QSBNetworkManager.OnNetworkManagerReady.AddListener(Init);
            }
        }

        private void Init()
        {
            var message = (T)Activator.CreateInstance(typeof(T));
            NetworkServer.RegisterHandler((short)message.MessageType, OnServerReceiveMessageHandler);
            NetworkManager.singleton.client.RegisterHandler((short)message.MessageType, OnClientReceiveMessageHandler);
        }

        public void SendToAll(T message)
        {
            if (!QSBNetworkManager.IsReady)
            {
                return;
            }
            NetworkServer.SendByChannelToAll((short)message.MessageType, message, QSBNetworkManager.ReliableChannelId);
        }

        public void SendToServer(T message)
        {
            if (!QSBNetworkManager.IsReady)
            {
                return;
            }
            NetworkManager.singleton.client.SendByChannel((short)message.MessageType, message, QSBNetworkManager.ReliableChannelId);
        }

        private void OnClientReceiveMessageHandler(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<T>();
            OnClientReceiveMessage?.Invoke(message);
        }

        private void OnServerReceiveMessageHandler(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<T>();
            OnServerReceiveMessage?.Invoke(message);
        }

    }
}
