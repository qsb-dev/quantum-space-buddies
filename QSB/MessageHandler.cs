using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public abstract class MessageHandler: MonoBehaviour {
        public abstract short type { get; }

        public MessageHandler () {
            NetworkServer.RegisterHandler(SectorMessage.Type, OnReceiveMessage);
            NetworkManager.singleton.client.RegisterHandler(SectorMessage.Type, OnReceiveMessage);
        }

        protected abstract void OnReceiveMessage (NetworkMessage netMsg);
    }
}
