using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public abstract class MessageHandler: MonoBehaviour {
        protected abstract short type { get; }

        void Awake () {
            NetworkServer.RegisterHandler(type, OnServerReceiveMessage);
            NetworkManager.singleton.client.RegisterHandler(type, OnClientReceiveMessage);
        }

        protected abstract void OnClientReceiveMessage (NetworkMessage netMsg);
        protected abstract void OnServerReceiveMessage (NetworkMessage netMsg);
    }
}
