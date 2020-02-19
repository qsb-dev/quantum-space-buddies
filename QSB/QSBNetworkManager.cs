using QSB.Animation;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class QSBNetworkManager : NetworkManager
    {
        private void Awake()
        {
            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");
            playerPrefab = assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<NetworkPlayer>();
            playerPrefab.AddComponent<AnimationSync>();
        }

        public override void OnStartServer()
        {
            WakeUpSync.IsServer = true;
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            DebugLog.Screen("OnClientConnect");
            gameObject.AddComponent<WakeUpSync>();
            gameObject.AddComponent<SectorSync>();
        }

    }
}
