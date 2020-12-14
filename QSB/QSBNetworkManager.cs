using OWML.Common;
using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.DeathSync;
using QSB.ElevatorSync;
using QSB.Events;
using QSB.GeyserSync;
using QSB.Instruments;
using QSB.OrbSync;
using QSB.Patches;
using QSB.Player;
using QSB.SectorSync;
using QSB.TimeSync;
using QSB.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Components;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
	public class QSBNetworkManager : QSBNetworkManagerUNET
	{
		private const int MaxConnections = 128;
		private const int MaxBufferedPackets = 64;

		public static QSBNetworkManager Instance { get; private set; }

		public event Action OnNetworkManagerReady;

		public bool IsReady { get; private set; }

		private QSBNetworkLobby _lobby;
		private AssetBundle _assetBundle;
		private GameObject _shipPrefab;
		private GameObject _cameraPrefab;
		private GameObject _probePrefab;
		public GameObject OrbPrefab;

		private void Awake()
		{
			Instance = this;

			_lobby = gameObject.AddComponent<QSBNetworkLobby>();
			_assetBundle = QSBCore.NetworkAssetBundle;

			playerPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
			var ident = playerPrefab.AddComponent<QSBNetworkIdentity>();
			ident.LocalPlayerAuthority = true;
			ident.SetValue("m_AssetId", playerPrefab.GetComponent<NetworkIdentity>().assetId);
			ident.SetValue("m_SceneId", playerPrefab.GetComponent<NetworkIdentity>().sceneId);
			Destroy(playerPrefab.GetComponent<NetworkTransform>());
			Destroy(playerPrefab.GetComponent<NetworkIdentity>());
			var transform = playerPrefab.AddComponent<QSBNetworkTransform>();
			transform.SendInterval = 0.1f;
			transform.SyncRotationAxis = QSBNetworkTransform.AxisSyncMode.AxisXYZ;
			playerPrefab.AddComponent<PlayerTransformSync>();
			playerPrefab.AddComponent<AnimationSync>();
			playerPrefab.AddComponent<WakeUpSync>();
			playerPrefab.AddComponent<InstrumentsManager>();

			_shipPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkship.prefab");
			ident = _shipPrefab.AddComponent<QSBNetworkIdentity>();
			ident.LocalPlayerAuthority = true;
			ident.SetValue("m_AssetId", _shipPrefab.GetComponent<NetworkIdentity>().assetId);
			ident.SetValue("m_SceneId", _shipPrefab.GetComponent<NetworkIdentity>().sceneId);
			Destroy(_shipPrefab.GetComponent<NetworkTransform>());
			Destroy(_shipPrefab.GetComponent<NetworkIdentity>());
			transform = _shipPrefab.AddComponent<QSBNetworkTransform>();
			transform.SendInterval = 0.1f;
			transform.SyncRotationAxis = QSBNetworkTransform.AxisSyncMode.AxisXYZ;
			_shipPrefab.AddComponent<ShipTransformSync>();
			spawnPrefabs.Add(_shipPrefab);

			_cameraPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkcameraroot.prefab");
			ident = _cameraPrefab.AddComponent<QSBNetworkIdentity>();
			ident.LocalPlayerAuthority = true;
			ident.SetValue("m_AssetId", _cameraPrefab.GetComponent<NetworkIdentity>().assetId);
			ident.SetValue("m_SceneId", _cameraPrefab.GetComponent<NetworkIdentity>().sceneId);
			Destroy(_cameraPrefab.GetComponent<NetworkTransform>());
			Destroy(_cameraPrefab.GetComponent<NetworkIdentity>());
			transform = _cameraPrefab.AddComponent<QSBNetworkTransform>();
			transform.SendInterval = 0.1f;
			transform.SyncRotationAxis = QSBNetworkTransform.AxisSyncMode.AxisXYZ;
			_cameraPrefab.AddComponent<PlayerCameraSync>();
			spawnPrefabs.Add(_cameraPrefab);

			_probePrefab = _assetBundle.LoadAsset<GameObject>("assets/networkprobe.prefab");
			ident = _probePrefab.AddComponent<QSBNetworkIdentity>();
			ident.LocalPlayerAuthority = true;
			ident.SetValue("m_AssetId", _probePrefab.GetComponent<NetworkIdentity>().assetId);
			ident.SetValue("m_SceneId", _probePrefab.GetComponent<NetworkIdentity>().sceneId);
			Destroy(_probePrefab.GetComponent<NetworkTransform>());
			Destroy(_probePrefab.GetComponent<NetworkIdentity>());
			transform = _probePrefab.AddComponent<QSBNetworkTransform>();
			transform.SendInterval = 0.1f;
			transform.SyncRotationAxis = QSBNetworkTransform.AxisSyncMode.AxisXYZ;
			_probePrefab.AddComponent<PlayerProbeSync>();
			spawnPrefabs.Add(_probePrefab);

			OrbPrefab = _assetBundle.LoadAsset<GameObject>("assets/networkorb.prefab");
			ident = OrbPrefab.AddComponent<QSBNetworkIdentity>();
			ident.LocalPlayerAuthority = true;
			ident.SetValue("m_AssetId", OrbPrefab.GetComponent<NetworkIdentity>().assetId);
			ident.SetValue("m_SceneId", OrbPrefab.GetComponent<NetworkIdentity>().sceneId);
			Destroy(OrbPrefab.GetComponent<NetworkTransform>());
			Destroy(OrbPrefab.GetComponent<NetworkIdentity>());
			transform = OrbPrefab.AddComponent<QSBNetworkTransform>();
			transform.SendInterval = 0.1f;
			transform.SyncRotationAxis = QSBNetworkTransform.AxisSyncMode.AxisXYZ;
			OrbPrefab.AddComponent<NomaiOrbTransformSync>();
			spawnPrefabs.Add(OrbPrefab);

			ConfigureNetworkManager();
			QSBSceneManager.OnUniverseSceneLoaded += OnSceneLoaded;
		}

		private void OnDestroy()
			=> QSBSceneManager.OnUniverseSceneLoaded -= OnSceneLoaded;

		private void OnSceneLoaded(OWScene scene)
		{
			OrbManager.Instance.BuildOrbs();
			OrbManager.Instance.QueueBuildSlots();
			QSBWorldSync.OldDialogueTrees.Clear();
			QSBWorldSync.OldDialogueTrees = Resources.FindObjectsOfTypeAll<CharacterDialogueTree>().ToList();
		}

		private void ConfigureNetworkManager()
		{
			networkAddress = QSBCore.DefaultServerIP;
			networkPort = QSBCore.Port;
			maxConnections = MaxConnections;
			customConfig = true;
			connectionConfig.AddChannel(QosType.Reliable);
			connectionConfig.AddChannel(QosType.Unreliable);
			this.SetValue("m_MaxBufferedPackets", MaxBufferedPackets);
			channels.Add(QosType.Reliable);
			channels.Add(QosType.Unreliable);

			DebugLog.DebugWrite("Network Manager ready.", MessageType.Success);
		}

		public override void OnStartServer()
		{
			DebugLog.DebugWrite("OnStartServer", MessageType.Info);
			if (QSBWorldSync.OrbSyncList.Count == 0 && QSBSceneManager.IsInUniverse)
			{
				OrbManager.Instance.QueueBuildOrbs();
			}
			if (QSBWorldSync.OldDialogueTrees.Count == 0 && QSBSceneManager.IsInUniverse)
			{
				QSBWorldSync.OldDialogueTrees = Resources.FindObjectsOfTypeAll<CharacterDialogueTree>().ToList();
			}
		}

		public override void OnServerAddPlayer(QSBNetworkConnection connection, short playerControllerId) // Called on the server when a client joins
		{
			DebugLog.DebugWrite($"OnServerAddPlayer {playerControllerId}", MessageType.Info);
			base.OnServerAddPlayer(connection, playerControllerId);

			QSBNetworkServer.SpawnWithClientAuthority(Instantiate(_shipPrefab), connection);
			QSBNetworkServer.SpawnWithClientAuthority(Instantiate(_cameraPrefab), connection);
			QSBNetworkServer.SpawnWithClientAuthority(Instantiate(_probePrefab), connection);
		}

		public override void OnClientConnect(QSBNetworkConnection connection) // Called on the client when connecting to a server
		{
			DebugLog.DebugWrite("OnClientConnect", MessageType.Info);
			base.OnClientConnect(connection);

			QSBEventManager.Init();

			gameObject.AddComponent<SectorSync.SectorSync>();
			gameObject.AddComponent<RespawnOnDeath>();
			gameObject.AddComponent<PreventShipDestruction>();

			if (QSBSceneManager.IsInUniverse)
			{
				QSBSectorManager.Instance.RebuildSectors();
				OrbManager.Instance.QueueBuildSlots();
			}

			if (!QSBNetworkServer.localClientActive)
			{
				QSBPatchManager.DoPatchType(QSBPatchTypes.OnNonServerClientConnect);
			}

			QSBPatchManager.DoPatchType(QSBPatchTypes.OnClientConnect);

			_lobby.CanEditName = false;

			OnNetworkManagerReady?.Invoke();
			IsReady = true;

			QSBCore.Helper.Events.Unity.RunWhen(() => QSBEventManager.Ready && PlayerTransformSync.LocalInstance != null,
				() => GlobalMessenger<string>.FireEvent(EventNames.QSBPlayerJoin, _lobby.PlayerName));

			if (!QSBCore.IsServer)
			{
				QSBCore.Helper.Events.Unity.RunWhen(() => QSBEventManager.Ready && PlayerTransformSync.LocalInstance != null,
				() => GlobalMessenger.FireEvent(EventNames.QSBPlayerStatesRequest));
			}
		}

		public override void OnStopClient() // Called on the client when closing connection
		{
			DebugLog.DebugWrite("OnStopClient", MessageType.Info);
			DebugLog.ToConsole("Disconnecting from server...", MessageType.Info);
			Destroy(GetComponent<SectorSync.SectorSync>());
			Destroy(GetComponent<RespawnOnDeath>());
			Destroy(GetComponent<PreventShipDestruction>());
			QSBEventManager.Reset();
			QSBPlayerManager.PlayerList.ForEach(player => player.HudMarker?.Remove());

			QSBPlayerManager.RemoveAllPlayers();

			QSBWorldSync.RemoveWorldObjects<QSBOrbSlot>();
			QSBWorldSync.RemoveWorldObjects<QSBElevator>();
			QSBWorldSync.RemoveWorldObjects<QSBGeyser>();
			QSBWorldSync.RemoveWorldObjects<QSBSector>();
			QSBWorldSync.OrbSyncList.Clear();
			QSBWorldSync.OldDialogueTrees.Clear();

			_lobby.CanEditName = true;
		}

		public override void OnServerDisconnect(QSBNetworkConnection connection) // Called on the server when any client disconnects
		{
			base.OnServerDisconnect(connection);
			DebugLog.DebugWrite("OnServerDisconnect", MessageType.Info);

			foreach (var item in QSBWorldSync.OrbSyncList)
			{
				var identity = item.GetComponent<QSBNetworkIdentity>();
				if (identity.ClientAuthorityOwner == connection)
				{
					identity.RemoveClientAuthority(connection);
				}
			}
		}

		public override void OnStopServer()
		{
			DebugLog.DebugWrite("OnStopServer", MessageType.Info);
			Destroy(GetComponent<SectorSync.SectorSync>());
			Destroy(GetComponent<RespawnOnDeath>());
			Destroy(GetComponent<PreventShipDestruction>());
			QSBEventManager.Reset();
			DebugLog.ToConsole("Server stopped!", MessageType.Info);
			QSBPlayerManager.PlayerList.ForEach(player => player.HudMarker?.Remove());

			QSBWorldSync.RemoveWorldObjects<QSBOrbSlot>();
			QSBWorldSync.RemoveWorldObjects<QSBElevator>();
			QSBWorldSync.RemoveWorldObjects<QSBGeyser>();
			QSBWorldSync.RemoveWorldObjects<QSBSector>();

			base.OnStopServer();
		}
	}
}