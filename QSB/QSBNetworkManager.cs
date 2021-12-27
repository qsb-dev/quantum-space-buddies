using OWML.Common;
using QSB.Anglerfish.TransformSync;
using QSB.AuthoritySync;
using QSB.ClientServerStateSync;
using QSB.DeathSync;
using QSB.JellyfishSync.TransformSync;
using QSB.Messaging;
using QSB.OrbSync.Messages;
using QSB.OrbSync.TransformSync;
using QSB.OrbSync.WorldObjects;
using QSB.Patches;
using QSB.Player;
using QSB.Player.Messages;
using QSB.Player.TransformSync;
using QSB.PoolSync;
using QSB.ShipSync.TransformSync;
using QSB.TimeSync;
using QSB.Tools.ProbeTool.TransformSync;
using QSB.TornadoSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Components;
using System;
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
		public GameObject OccasionalPrefab { get; private set; }
		public string PlayerName { get; private set; }

		private const int MaxConnections = 128;
		private const int MaxBufferedPackets = 64;

		private GameObject _probePrefab;
		private bool _everConnected;

		public new void Awake()
		{
			base.Awake();
			Instance = this;

			PlayerName = GetPlayerName();

			playerPrefab = QSBCore.NetworkAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/NETWORK_Player_Body.prefab");

			ShipPrefab = MakeNewNetworkObject(2, "NetworkShip", typeof(ShipTransformSync));
			spawnPrefabs.Add(ShipPrefab);

			_probePrefab = MakeNewNetworkObject(3, "NetworkProbe", typeof(PlayerProbeSync));
			spawnPrefabs.Add(_probePrefab);

			OrbPrefab = MakeNewNetworkObject(4, "NetworkOrb", typeof(NomaiOrbTransformSync));
			spawnPrefabs.Add(OrbPrefab);

			AnglerPrefab = MakeNewNetworkObject(5, "NetworkAngler", typeof(AnglerTransformSync));
			spawnPrefabs.Add(AnglerPrefab);

			JellyfishPrefab = MakeNewNetworkObject(6, "NetworkJellyfish", typeof(JellyfishTransformSync));
			spawnPrefabs.Add(JellyfishPrefab);

			OccasionalPrefab = MakeNewNetworkObject(7, "NetworkOccasional", typeof(OccasionalTransformSync));
			spawnPrefabs.Add(OccasionalPrefab);

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

		/// create a new network prefab from the network object prefab template.
		/// this works by calling Unload(false) and then reloading the AssetBundle,
		/// which makes LoadAsset give you a new resource.
		/// see https://docs.unity3d.com/Manual/AssetBundles-Native.html.
		private static GameObject MakeNewNetworkObject(int assetId, string name, Type transformSyncType)
		{
			QSBCore.NetworkAssetBundle.Unload(false);
			QSBCore.NetworkAssetBundle = QSBCore.Helper.Assets.LoadBundle("AssetBundles/network");

			var template = QSBCore.NetworkAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/NetworkObject.prefab");
			DebugLog.DebugWrite($"MakeNewNetworkObject - prefab id {template.GetInstanceID()} "
				+ $"for {assetId} {name} {transformSyncType.Name}");
			template.name = name;
			template.GetRequiredComponent<QNetworkIdentity>().m_AssetId = assetId;
			template.AddComponent(transformSyncType);
			return template;
		}

		private void ConfigureNetworkManager()
		{
			networkAddress = QSBCore.DefaultServerIP;
			networkPort = QSBCore.Port;
			maxConnections = MaxConnections;
			customConfig = true;
			connectionConfig.AddChannel(QosType.Reliable);
			connectionConfig.AddChannel(QosType.Unreliable);

			m_MaxBufferedPackets = MaxBufferedPackets;
			channels.Add(QosType.Reliable);
			channels.Add(QosType.Unreliable);

			DebugLog.DebugWrite("Network Manager ready.", MessageType.Success);
		}

		public override void OnStartServer()
		{
			DebugLog.DebugWrite("OnStartServer", MessageType.Info);
			if (QSBWorldSync.OldDialogueTrees.Count == 0 && QSBSceneManager.IsInUniverse)
			{
				QSBWorldSync.OldDialogueTrees.AddRange(QSBWorldSync.GetUnityObjects<CharacterDialogueTree>());
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

			QSBCore.UnityEvents.RunWhen(() => PlayerTransformSync.LocalInstance,
				() => new PlayerJoinMessage(PlayerName).Send());

			if (!QSBCore.IsHost)
			{
				QSBCore.UnityEvents.RunWhen(() => PlayerTransformSync.LocalInstance,
					() => new RequestStateResyncMessage().Send());
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
			QSBPlayerManager.PlayerList.ForEach(player => player.HudMarker?.Remove());

			RemoveWorldObjects();
			QSBWorldSync.DialogueConditions.Clear();
			QSBWorldSync.OldDialogueTrees.Clear();
			QSBWorldSync.ShipLogFacts.Clear();

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

			// revert authority from ship
			if (ShipTransformSync.LocalInstance)
			{
				var identity = ShipTransformSync.LocalInstance.NetIdentity;
				if (identity.ClientAuthorityOwner == conn)
				{
					identity.SetAuthority(QSBPlayerManager.LocalPlayerId);
				}
			}

			// stop dragging for the orbs this player was dragging
			foreach (var qsbOrb in QSBWorldSync.GetWorldObjects<QSBOrb>())
			{
				if (!qsbOrb.TransformSync.enabled)
				{
					continue;
				}

				var identity = qsbOrb.TransformSync.NetIdentity;
				if (identity.ClientAuthorityOwner == conn)
				{
					qsbOrb.SetDragging(false);
					qsbOrb.SendMessage(new OrbDragMessage(false));
				}
			}

			AuthorityManager.OnDisconnect(conn.GetPlayerId());

			base.OnServerDisconnect(conn);
		}

		public override void OnStopServer()
		{
			DebugLog.DebugWrite("OnStopServer", MessageType.Info);
			Destroy(GetComponent<RespawnOnDeath>());
			DebugLog.ToConsole("Server stopped!", MessageType.Info);
			QSBPlayerManager.PlayerList.ForEach(player => player.HudMarker?.Remove());

			base.OnStopServer();
		}

		private static void RemoveWorldObjects()
		{
			QSBWorldSync.RemoveWorldObjects();
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
