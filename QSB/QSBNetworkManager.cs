using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class QSBNetworkManager : NetworkManager
    {
        AssetBundle _assetBundle;
        private void Awake()
        {
            _assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");
            playerPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<NetworkPlayer>();
            playerPrefab.AddComponent<AnimationSync>();

            var prefab = _assetBundle.LoadAsset<GameObject>("assets/networkship.prefab");
            //prefab.AddComponent<ShipTransformSync>();
            spawnPrefabs.Add(prefab);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            base.OnServerAddPlayer(conn, playerControllerId);

            var prefab = _assetBundle.LoadAsset<GameObject>("assets/networkship.prefab");
            prefab.AddComponent<ShipTransformSync>();
            NetworkServer.SpawnWithClientAuthority(Instantiate(spawnPrefabs[0]), conn);
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
