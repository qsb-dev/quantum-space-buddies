using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    class QSBNetworkManager: NetworkManager {
        void Awake () {
            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");
            playerPrefab = assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<NetworkPlayer>();
        }

        public override void OnStartClient (NetworkClient client) {
            base.OnStartClient(client);

            gameObject.AddComponent<SectorSync>();
        }
    }
}
