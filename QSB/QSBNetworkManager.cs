using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    class QSBNetworkManager: NetworkManager {
        void Awake () {
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);

            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");
            playerPrefab = assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<NetworkPlayer>();
        }

        void OnWakeUp () {
            gameObject.AddComponent<SectorSync>();
        }
    }
}
