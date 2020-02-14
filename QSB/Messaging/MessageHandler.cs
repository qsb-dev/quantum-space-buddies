using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    // Extend this to create new message handlers.
    // You'll also need to create a new message type (add it to the enum).
    public abstract class MessageHandler: MonoBehaviour {
        protected abstract MessageType type { get; }

        void Awake () {
            NetworkServer.RegisterHandler((short) type, OnServerReceiveMessage);
            NetworkManager.singleton.client.RegisterHandler((short) type, OnClientReceiveMessage);
        }

        protected abstract void OnClientReceiveMessage (NetworkMessage netMsg);
        protected abstract void OnServerReceiveMessage (NetworkMessage netMsg);
    }
}
