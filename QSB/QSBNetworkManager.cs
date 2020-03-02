using QSB.Animation;
using QSB.TimeSync;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class QSBNetworkManager : NetworkManager
    {
        private const int MaxConnections = 128;

        private AssetBundle _assetBundle;
        private GameObject _shipPrefab;

        private void Awake()
        {
            _assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");
            playerPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<PlayerTransformSync>();
            playerPrefab.AddComponent<AnimationSync>();
            playerPrefab.AddComponent<WakeUpSync>();

            _shipPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkship.prefab");
            _shipPrefab.AddComponent<ShipTransformSync>();
            spawnPrefabs.Add(_shipPrefab);

            networkAddress = QSB.DefaultServerIP;
            maxConnections = MaxConnections;
            customConfig = true;
            connectionConfig.AddChannel(QosType.Reliable);
            connectionConfig.AddChannel(QosType.Unreliable);
            channels.Add(QosType.Reliable);
            channels.Add(QosType.Unreliable);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            base.OnServerAddPlayer(conn, playerControllerId);

            NetworkServer.SpawnWithClientAuthority(Instantiate(_shipPrefab), conn);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            DebugLog.Screen("OnClientConnect");
            gameObject.AddComponent<SectorSync>();
        }

    }
}
