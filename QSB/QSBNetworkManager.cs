using System;
using System.Linq;
using QSB.Animation;
using QSB.DeathSync;
using QSB.Events;
using QSB.GeyserSync;
using QSB.TimeSync;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class QSBNetworkManager : NetworkManager
    {
        private const int MaxConnections = 128;

        public static QSBNetworkManager Instance { get; private set; }

        public event Action OnNetworkManagerReady;
        public bool IsReady { get; private set; }

        private QSBNetworkLobby _lobby;
        private AssetBundle _assetBundle;
        private GameObject _shipPrefab;
        private GameObject _cameraPrefab;
        private GameObject _probePrefab;

        private void Awake()
        {
            Instance = this;

            _lobby = gameObject.AddComponent<QSBNetworkLobby>();
            _assetBundle = QSB.NetworkAssetBundle;

            playerPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<PlayerTransformSync>();
            playerPrefab.AddComponent<AnimationSync>();
            playerPrefab.AddComponent<WakeUpSync>();

            _shipPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkship.prefab");
            _shipPrefab.AddComponent<ShipTransformSync>();
            spawnPrefabs.Add(_shipPrefab);

            _cameraPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkcameraroot.prefab");
            _cameraPrefab.AddComponent<PlayerCameraSync>();
            spawnPrefabs.Add(_cameraPrefab);

            _probePrefab = _assetBundle.LoadAsset<GameObject>("assets/networkprobe.prefab");
            _probePrefab.AddComponent<PlayerProbeSync>();
            spawnPrefabs.Add(_probePrefab);

            ConfigureNetworkManager();
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

        public override void OnServerAddPlayer(NetworkConnection connection, short playerControllerId) // Called on the server when a client joins
        {
            DebugLog.ToConsole("On server add player " + playerControllerId);
            base.OnServerAddPlayer(connection, playerControllerId);

            // These have to be in a constant order (for now, until I get a better netId getting system...)
            NetworkServer.SpawnWithClientAuthority(Instantiate(_shipPrefab), connection);
            NetworkServer.SpawnWithClientAuthority(Instantiate(_cameraPrefab), connection);
            NetworkServer.SpawnWithClientAuthority(Instantiate(_probePrefab), connection);

            gameObject.AddComponent<Events.PlayerState>();
        }

        public override void OnClientConnect(NetworkConnection connection) // Called on the client when connecting to a server
        {
            base.OnClientConnect(connection);

            gameObject.AddComponent<SectorSync>();
            gameObject.AddComponent<RespawnOnDeath>();
            gameObject.AddComponent<PreventShipDestruction>();

            if (NetworkClient.active && !NetworkServer.active)
            {
                gameObject.AddComponent<Events.PlayerState>();
                GeyserManager.Instance.EmptyUpdate();
                WakeUpPatches.AddPatches();
            }

            _lobby.CanEditName = false;

            OnNetworkManagerReady.Invoke();
            IsReady = true;

            UnityHelper.Instance.RunWhen(() => PlayerTransformSync.LocalInstance != null, EventList.Init);

            UnityHelper.Instance.RunWhen(() => EventList.Ready,
                () => GlobalMessenger<string>.FireEvent(EventNames.QSBPlayerJoin, _lobby.PlayerName));
        }

        public override void OnStopClient() // Called on the client when closing connection
        {
            DebugLog.ToConsole("Disconnecting from server...", OWML.Common.MessageType.Info);
            Destroy(GetComponent<SectorSync>());
            Destroy(GetComponent<RespawnOnDeath>());
            Destroy(GetComponent<PreventShipDestruction>());
            EventList.Reset();
            PlayerRegistry.PlayerList.ForEach(player => player.HudMarker?.Remove());
            NetworkServer.connections.ToList()Where(x => x.playerControllers[0].gameObject.GetComponent<PlayerTransformSync>().netId.Value != PlayerRegistry.LocalPlayerId).ForEach(CleanupConnection);
            _lobby.CanEditName = true;
        }

        public override void OnServerDisconnect(NetworkConnection connection) // Called on the server when any client disconnects
        {
            var playerId = connection.playerControllers[0].gameObject.GetComponent<PlayerTransformSync>().netId.Value;
            var netIds = connection.clientOwnedObjects.Select(x => x.Value).ToArray();
            GlobalMessenger<uint, uint[]>.FireEvent(EventNames.QSBPlayerLeave, playerId, netIds);
			PlayerRegistry.GetPlayer(playerId).HudMarker?.Remove();
            CleanupConnection(connection);
        }

        public override void OnStopServer()
        {
            DebugLog.ToConsole("Server stopped!", OWML.Common.MessageType.Info);
            NetworkServer.connections.ToList().ForEach(CleanupConnection);
            base.OnStopServer();
        }

        private void CleanupConnection(NetworkConnection connection)
        {
            var playerId = connection.playerControllers[0].gameObject.GetComponent<PlayerTransformSync>().netId.Value;
            var playerName = PlayerRegistry.GetPlayer(playerId).Name;
            DebugLog.ToConsole($"{playerName} disconnected.", OWML.Common.MessageType.Info);
            PlayerRegistry.RemovePlayer(playerId);

            var netIds = connection.clientOwnedObjects.Select(x => x.Value).ToList();
            netIds.ForEach(CleanupNetworkBehaviour);
        }

        public void CleanupNetworkBehaviour(uint netId)
        {
            var networkBehaviours = FindObjectsOfType<NetworkBehaviour>()
                .Where(x => x.netId.Value == netId);
            foreach (var networkBehaviour in networkBehaviours)
            {
                if (networkBehaviour == null)
                {
                    continue;
                }
                var transformSync = networkBehaviour.GetComponent<TransformSync.TransformSync>();

                if (transformSync != null)
                {
                    DebugLog.ToConsole("    * TS is not null - removing from list");
                    PlayerRegistry.TransformSyncs.Remove(transformSync);
                    if (transformSync.SyncedTransform != null)
                    {
                        DebugLog.ToConsole("    * TS's ST is not null - destroying");
                        Destroy(transformSync.SyncedTransform.gameObject);
                    }
                }
                Destroy(networkBehaviour.gameObject);
            }
        }

    }
}
