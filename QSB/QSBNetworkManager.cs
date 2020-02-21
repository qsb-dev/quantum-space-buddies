using QSB.Animation;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class QSBNetworkManager : NetworkManager
    {
        private AssetBundle _assetBundle;
        private GameObject _shipPrefab;

        private void Awake()
        {
            _assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");
            playerPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<NetworkPlayer>();
            playerPrefab.AddComponent<AnimationSync>();

            _shipPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkship.prefab");
            _shipPrefab.AddComponent<ShipTransformSync>();
            spawnPrefabs.Add(_shipPrefab);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            base.OnServerAddPlayer(conn, playerControllerId);

            NetworkServer.SpawnWithClientAuthority(Instantiate(_shipPrefab), conn);
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
