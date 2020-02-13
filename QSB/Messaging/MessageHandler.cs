using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public abstract class MessageHandler: MonoBehaviour {
        protected abstract short type { get; }

        public MessageHandler () {
            NetworkServer.RegisterHandler(MessageType.Sector, OnServerReceiveMessage);
            NetworkManager.singleton.client.RegisterHandler(MessageType.Sector, OnClientReceiveMessage);
        }

        protected abstract void OnClientReceiveMessage (NetworkMessage netMsg);
        protected abstract void OnServerReceiveMessage (NetworkMessage netMsg);
    }
}
