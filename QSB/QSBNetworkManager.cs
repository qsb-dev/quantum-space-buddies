using System;
using System.Linq;
using QSB.Animation;
using QSB.Events;
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

        private readonly string[] _defaultNames = {
            "Arkose",
            "Chert",
            "Esker",
            "Hal",
            "Hornfels",
            "Feldspar",
            "Gabbro",
            "Galena",
            "Gneiss",
            "Gossan",
            "Marl",
            "Mica",
            "Moraine",
            "Porphy",
            "Riebeck",
            "Rutile",
            "Slate",
            "Spinel",
            "Tektite",
            "Tephra",
            "Tuff"
        };
        private string _playerName;

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

            ConfigureNetworkManager();

            _playerName = _defaultNames.OrderBy(x => Guid.NewGuid()).First();
        }

        private void ConfigureNetworkManager()
        {
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
            gameObject.AddComponent<PlayerJoin>().Join(_playerName);
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200f, 20f), "Name:");
            _playerName = GUI.TextField(new Rect(60, 10, 145, 20f), _playerName);
        }

    }
}
