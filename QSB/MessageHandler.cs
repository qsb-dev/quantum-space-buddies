using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public abstract class MessageHandler: MonoBehaviour {
        protected abstract short type { get; }

        public MessageHandler () {
            NetworkServer.RegisterHandler(SectorMessage.Type, OnServerReceiveMessage);
            NetworkManager.singleton.client.RegisterHandler(SectorMessage.Type, OnClientReceiveMessage);
        }

        protected abstract void OnClientReceiveMessage (NetworkMessage netMsg);
        protected abstract void OnServerReceiveMessage (NetworkMessage netMsg);
    }
}
