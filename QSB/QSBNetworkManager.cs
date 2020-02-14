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

        public override void OnClientConnect (NetworkConnection conn) {
            base.OnClientConnect(conn);

            DebugLog.Screen("OnClientConnect");
            gameObject.AddComponent<WakeUpSync>();
            gameObject.AddComponent<SectorSync>();
        }

        void OnWakeUp () {
        }
    }
}
