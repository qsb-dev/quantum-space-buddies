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
        public string _playerName;
        public bool _canEditName;

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
            maxConnections = MaxConnections;
            customConfig = true;
            connectionConfig.AddChannel(QosType.Reliable);
            connectionConfig.AddChannel(QosType.Unreliable);
            channels.Add(QosType.Reliable);
            channels.Add(QosType.Unreliable);

            //QSB.Helper.HarmonyHelper.EmptyMethod<NetworkManagerHUD>("Update");
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
            gameObject.AddComponent<PlayerLeave>();
            gameObject.AddComponent<RespawnOnDeath>();
            gameObject.AddComponent<PreventShipDestruction>();

            _canEditName = false;

            OnNetworkManagerReady.Invoke();
            IsReady = true;
        }

        public override void OnStopClient()
        {
            // Stop client without errors

            DebugLog.Screen("OnStopClient");
            var sectorSync = GetComponent<SectorSync>();
            if(sectorSync != null)
                Destroy(sectorSync);

            var playerJoin = GetComponent<PlayerJoin>();
            if (playerJoin != null)
                Destroy(playerJoin);

            var playerLeave = GetComponent<PlayerLeave>();
            if (playerLeave != null)
                Destroy(playerLeave);

            var respawnOnDeath = GetComponent<RespawnOnDeath>();
            if (respawnOnDeath != null)
                Destroy(respawnOnDeath);

            var preventShipDestruction = GetComponent<PreventShipDestruction>();
            if (preventShipDestruction != null)
                Destroy(preventShipDestruction);

            if (PlayerTransformSync.LocalInstance != null)
            {
                var animationSync = PlayerTransformSync.LocalInstance.gameObject.GetComponent<AnimationSync>();
                if (animationSync != null)
                    animationSync.Reset();
            }

            _canEditName = true;
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            DebugLog.Screen("OnServerDisconnect");

            var playerId = conn.playerControllers[0].gameObject.GetComponent<PlayerTransformSync>().netId.Value;
            var objectIds = conn.clientOwnedObjects.Select(x => x.Value).ToArray();
            GetComponent<PlayerLeave>().Leave(playerId, objectIds);

            base.OnServerDisconnect(conn);
        }
#if DEBUG
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
#endif
    }
}
