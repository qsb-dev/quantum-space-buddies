using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    // Extend this MonoBehaviour and set the message type 
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
