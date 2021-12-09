using System;
using System.Linq;
using OWML.Common;
using OWML.Utils;
using QSB.AuthoritySync;
using QSB.ClientServerStateSync;
using QSB.DeathSync;
using QSB.Events;
using QSB.Messaging;
using QSB.OrbSync.TransformSync;
using QSB.Patches;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.PoolSync;
using QSB.ShipSync.TransformSync;
using QSB.TimeSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
	public class QSBNetworkManager : QNetworkManager
	{
		public static QSBNetworkManager Instance { get; private set; }

		public event Action OnNetworkManagerReady;
		public event Action OnClientConnected;
		public event Action<NetworkError> OnClientDisconnected;
		public event Action<NetworkError> OnClientErrorThrown;

		public bool IsReady { get; private set; }
		public GameObject OrbPrefab { get; private set; }
		public GameObject ShipPrefab { get; private set; }
		public GameObject AnglerPrefab { get; private set; }
		public GameObject JellyfishPrefab { get; private set; }
		public string PlayerName { get; private set; }

		private const int MaxConnections = 128;
		private const int MaxBufferedPackets = 64;

		private AssetBundle _assetBundle;
		private GameObject _probePrefab;
		private bool _everConnected;

		public new void Awake()
		{
			base.Awake();
			Instance = this;

			PlayerName = GetPlayerName();
			_assetBundle = QSBCore.NetworkAssetBundle;

			playerPrefab = _assetBundle.LoadAsset<GameObject>("Assets/Prefabs/NETWORK_Player_Body.prefab");

			ShipPrefab = _assetBundle.LoadAsset<GameObject>("assets/Prefabs/networkship.prefab");
			spawnPrefabs.Add(ShipPrefab);

			_probePrefab = _assetBundle.LoadAsset<GameObject>("assets/Prefabs/networkprobe.prefab");
			spawnPrefabs.Add(_probePrefab);

			OrbPrefab = _assetBundle.LoadAsset<GameObject>("assets/Prefabs/networkorb.prefab");
			spawnPrefabs.Add(OrbPrefab);

			AnglerPrefab = _assetBundle.LoadAsset<GameObject>("assets/Prefabs/networkangler.prefab");
			spawnPrefabs.Add(AnglerPrefab);

			JellyfishPrefab = _assetBundle.LoadAsset<GameObject>("assets/Prefabs/networkjellyfish.prefab");
			spawnPrefabs.Add(JellyfishPrefab);

			ConfigureNetworkManager();
		}

		private string GetPlayerName()
		{
			try
			{
				var profileManager = StandaloneProfileManager.SharedInstance;
				profileManager.Initialize();
				var profile = profileManager._currentProfile;
				var profileName = profile.profileName;
				return profileName;
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error - Exception when getting player name : {ex}", MessageType.Error);
				return "Player";
			}
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
			if (QSBWorldSync.OldDialogueTrees.Count == 0 && QSBSceneManager.IsInUniverse)
			{
				QSBWorldSync.OldDialogueTrees = QSBWorldSync.GetUnityObjects<CharacterDialogueTree>().ToList();
			}
		}

		public override void OnServerAddPlayer(QNetworkConnection connection, short playerControllerId) // Called on the server when a client joins
		{
			DebugLog.DebugWrite($"OnServerAddPlayer {playerControllerId}", MessageType.Info);
			base.OnServerAddPlayer(connection, playerControllerId);

			QNetworkServer.SpawnWithClientAuthority(Instantiate(_probePrefab), connection);
		}

		public override void OnStartClient(QNetworkClient _)
		{
			var config = QSBCore.Helper.Config;
			config.SetSettingsValue("defaultServerIP", networkAddress);
			QSBCore.Helper.Storage.Save(config, Constants.ModConfigFileName);
		}

		public override void OnClientError(QNetworkConnection conn, int errorCode)
			=> OnClientErrorThrown?.SafeInvoke((NetworkError)errorCode);

		public override void OnClientConnect(QNetworkConnection connection) // Called on the client when connecting to a server
		{
			DebugLog.DebugWrite("OnClientConnect", MessageType.Info);
			base.OnClientConnect(connection);

			OnClientConnected?.SafeInvoke();

			QSBEventManager.Init();
			QSBMessageManager.Init();

			gameObject.AddComponent<RespawnOnDeath>();
			gameObject.AddComponent<ServerStateManager>();
			gameObject.AddComponent<ClientStateManager>();

			if (QSBSceneManager.IsInUniverse)
			{
				WorldObjectManager.Rebuild(QSBSceneManager.CurrentScene);
			}

			var specificType = QNetworkServer.active ? QSBPatchTypes.OnServerClientConnect : QSBPatchTypes.OnNonServerClientConnect;
			QSBPatchManager.DoPatchType(specificType);
			QSBPatchManager.DoPatchType(QSBPatchTypes.OnClientConnect);

			OnNetworkManagerReady?.SafeInvoke();
			IsReady = true;

			QSBCore.UnityEvents.RunWhen(() => QSBEventManager.Ready && PlayerTransformSync.LocalInstance != null,
				() => QSBEventManager.FireEvent(EventNames.QSBPlayerJoin, PlayerName));

			if (!QSBCore.IsHost)
			{
				QSBCore.UnityEvents.RunWhen(() => QSBEventManager.Ready && PlayerTransformSync.LocalInstance != null,
					() => QSBEventManager.FireEvent(EventNames.QSBRequestStateResync));
			}

			_everConnected = true;
		}

		public override void OnStopClient() // Called on the client when closing connection
		{
			DebugLog.DebugWrite("OnStopClient", MessageType.Info);
			DebugLog.ToConsole("Disconnecting from server...", MessageType.Info);
			Destroy(GetComponent<RespawnOnDeath>());
			Destroy(GetComponent<ServerStateManager>());
			Destroy(GetComponent<ClientStateManager>());
			QSBEventManager.Reset();
			QSBPlayerManager.PlayerList.ForEach(player => player.HudMarker?.Remove());

			RemoveWorldObjects();
			NomaiOrbTransformSync.OrbTransformSyncs.Clear();
			QSBWorldSync.OldDialogueTrees.Clear();

			if (WakeUpSync.LocalInstance != null)
			{
				WakeUpSync.LocalInstance.OnDisconnect();
			}

			if (_everConnected)
			{
				var specificType = QNetworkServer.active ? QSBPatchTypes.OnServerClientConnect : QSBPatchTypes.OnNonServerClientConnect;
				QSBPatchManager.DoUnpatchType(specificType);
				QSBPatchManager.DoUnpatchType(QSBPatchTypes.OnClientConnect);
			}

			IsReady = false;
			_everConnected = false;
		}

		public override void OnClientDisconnect(QNetworkConnection conn)
		{
			base.OnClientDisconnect(conn);
			OnClientDisconnected?.SafeInvoke(conn.LastError);
		}

		public override void OnServerDisconnect(QNetworkConnection conn) // Called on the server when any client disconnects
		{
			DebugLog.DebugWrite("OnServerDisconnect", MessageType.Info);

			// revert authority for orbs
			foreach (var item in NomaiOrbTransformSync.OrbTransformSyncs)
			{
				if (!item)
				{
					DebugLog.ToConsole($"Warning - null transform sync in NomaiOrbTransformSync.OrbTransformSyncs!", MessageType.Warning);
					continue;
				}

				var identity = item.NetIdentity;
				if (identity.ClientAuthorityOwner == conn)
				{
					identity.SetAuthority(QSBPlayerManager.LocalPlayerId);
				}
			}

			// revert authority from ship
			if (ShipTransformSync.LocalInstance)
			{
				var identity = ShipTransformSync.LocalInstance.NetIdentity;
				if (identity.ClientAuthorityOwner == conn)
				{
					identity.SetAuthority(QSBPlayerManager.LocalPlayerId);
				}
			}

			AuthorityManager.OnDisconnect(conn.GetPlayerId());

			base.OnServerDisconnect(conn);
		}

		public override void OnStopServer()
		{
			DebugLog.DebugWrite("OnStopServer", MessageType.Info);
			Destroy(GetComponent<RespawnOnDeath>());
			QSBEventManager.Reset();
			DebugLog.ToConsole("Server stopped!", MessageType.Info);
			QSBPlayerManager.PlayerList.ForEach(player => player.HudMarker?.Remove());

			base.OnStopServer();
		}

		private void RemoveWorldObjects()
		{
			QSBWorldSync.RemoveWorldObjects<IWorldObjectTypeSubset>();
			QSBWorldSync.RemoveWorldObjects<IWorldObject>();
			foreach (var platform in QSBWorldSync.GetUnityObjects<CustomNomaiRemoteCameraPlatform>())
			{
				Destroy(platform);
			}

			foreach (var camera in QSBWorldSync.GetUnityObjects<CustomNomaiRemoteCamera>())
			{
				Destroy(camera);
			}

			foreach (var streaming in QSBWorldSync.GetUnityObjects<CustomNomaiRemoteCameraStreaming>())
			{
				Destroy(streaming);
			}

			WorldObjectManager.SetNotReady();
		}
	}
}
