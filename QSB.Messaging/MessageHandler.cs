using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Messaging
{
    // Extend this to create new message handlers.
    // You'll also need to create a new message type (add it to the enum).
    public abstract class MessageHandler : MonoBehaviour
    {
        protected abstract MessageType Type { get; }

        private void Awake()
        {
            NetworkServer.RegisterHandler((short)Type, OnServerReceiveMessage);
            NetworkManager.singleton.client.RegisterHandler((short)Type, OnClientReceiveMessage);
        }

        protected abstract void OnClientReceiveMessage(NetworkMessage netMsg);
        protected abstract void OnServerReceiveMessage(NetworkMessage netMsg);
    }
}
