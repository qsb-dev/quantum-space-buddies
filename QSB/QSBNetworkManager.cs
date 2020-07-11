using System;
using System.Linq;
using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.Events;
using QSB.TimeSync;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace QSB
{
    public class QSBNetworkManager : NetworkManager
    {
        public static UnityEvent OnNetworkManagerReady = new UnityEvent();
        public static bool IsReady = false;

        private const int MAX_CONNECTIONS = 128;

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
        private bool _canEditName;

        private void Awake()
        {
            _assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");

            // Loads the network player prefab into the network manager, then adds scripts to the prefab.
            // For every player that then joins a new instance of the prefab is made, with those new scripts.
            playerPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<PlayerTransformSync>();
            playerPrefab.AddComponent<AnimationSync>();
            playerPrefab.AddComponent<WakeUpSync>();

            _shipPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkship.prefab");
            _shipPrefab.AddComponent<ShipTransformSync>();
            spawnPrefabs.Add(_shipPrefab);

            ConfigureNetworkManager();

            _playerName = GetPlayerName();
            _canEditName = true;
        }

        private string GetPlayerName()
        {
            var profileManager = StandaloneProfileManager.SharedInstance;
            profileManager.Initialize();
            var profile = profileManager.GetValue<StandaloneProfileManager.ProfileData>("_currentProfile");
            var profileName = profile?.profileName;
            if (!string.IsNullOrEmpty(profileName))
            {
                return profileName;
            }
            return _defaultNames.OrderBy(x => Guid.NewGuid()).First();
        }

        private void ConfigureNetworkManager()
        {
            networkAddress = QSB.DefaultServerIP;
            maxConnections = MAX_CONNECTIONS;
            customConfig = true;
            connectionConfig.AddChannel(QosType.Reliable);
            connectionConfig.AddChannel(QosType.Unreliable);
            channels.Add(QosType.Reliable);
            channels.Add(QosType.Unreliable);

            QSB.Helper.HarmonyHelper.EmptyMethod<NetworkManagerHUD>("Update");
        }

        public override void OnServerAddPlayer(NetworkConnection connection, short playerControllerId) // Called on the server when a client joins
        {
            base.OnServerAddPlayer(connection, playerControllerId);

            NetworkServer.SpawnWithClientAuthority(Instantiate(_shipPrefab), connection);

            var gameState = gameObject.AddComponent<GameState>();
            gameState.Send();
        }

        public override void OnClientConnect(NetworkConnection connection) // Called on the client when connecting to a server
        {
            base.OnClientConnect(connection);

            gameObject.AddComponent<SectorSync>();
            gameObject.AddComponent<PlayerJoin>().Join(_playerName);
            gameObject.AddComponent<PlayerLeave>();
            gameObject.AddComponent<RespawnOnDeath>();
            gameObject.AddComponent<PreventShipDestruction>();
            gameObject.AddComponent<Events.EventHandler>();

            if (!Network.isServer)
            {
                gameObject.AddComponent<GameState>();
            }

            _canEditName = false;

            OnNetworkManagerReady.Invoke();
            IsReady = true;
        }

        public override void OnStopClient() // Called on the client when closing connection
        {
            DebugLog.ToScreen("OnStopClient");
            Destroy(GetComponent<SectorSync>());
            Destroy(GetComponent<PlayerJoin>());
            Destroy(GetComponent<PlayerLeave>());
            Destroy(GetComponent<RespawnOnDeath>());
            Destroy(GetComponent<PreventShipDestruction>());
            Destroy(GetComponent<Events.EventHandler>());
            if (IsClientConnected())
            {
                PlayerTransformSync.LocalInstance.gameObject.GetComponent<AnimationSync>().Reset();
            }
            _canEditName = true;
        }

        public override void OnServerDisconnect(NetworkConnection connection) // Called on the server when any client disconnects
        {
            DebugLog.ToScreen("OnServerDisconnect");

            var playerId = connection.playerControllers[0].gameObject.GetComponent<PlayerTransformSync>().netId.Value;
            var objectIds = connection.clientOwnedObjects.Select(x => x.Value).ToArray();
            GetComponent<PlayerLeave>().Leave(playerId, objectIds);

            base.OnServerDisconnect(connection);
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200f, 20f), "Name:");
            if (_canEditName)
            {
                _playerName = GUI.TextField(new Rect(60, 10, 145, 20f), _playerName);
            }
            else
            {
                GUI.Label(new Rect(60, 10, 145, 20f), _playerName);
            }
        }

    }
}
