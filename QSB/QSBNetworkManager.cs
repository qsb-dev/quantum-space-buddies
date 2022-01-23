using Mirror;
using Mirror.FizzySteam;
using OWML.Common;
using OWML.Utils;
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
using QSB.ShipSync.TransformSync;
using QSB.TimeSync;
using QSB.Tools.ProbeTool.TransformSync;
using QSB.TornadoSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QSB
{
	public class QSBNetworkManager : NetworkManager
	{
		public new static QSBNetworkManager singleton => (QSBNetworkManager)NetworkManager.singleton;

		public event Action OnClientConnected;
		public event Action<string> OnClientDisconnected;

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

		private string _lastTransportError;
		internal bool _intentionalDisconnect;

		public override void Awake()
		{
			AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => x.GetName().Name.StartsWith("Mirror"))
				.Append(typeof(QSBNetworkManager).Assembly)
				.SelectMany(x => x.GetTypes())
				.SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
				.Where(x => x.GetCustomAttribute<RuntimeInitializeOnLoadMethodAttribute>() != null)
				.ForEach(x => x.Invoke(null, null));

			gameObject.SetActive(false);

			if (QSBCore.UseKcpTransport)
			{
				transport = gameObject.AddComponent<kcp2k.KcpTransport>();
			}
			else
			{
				var fizzy = gameObject.AddComponent<FizzyFacepunch>();
				fizzy.SteamAppID = QSBCore.OverrideAppId == -1
					? "753640"
					: $"{QSBCore.OverrideAppId}";
				transport = fizzy;
			}

			gameObject.SetActive(true);

			base.Awake();

			InitPlayerName();

			playerPrefab = QSBCore.NetworkAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/NETWORK_Player_Body.prefab");
			playerPrefab.GetRequiredComponent<NetworkIdentity>().SetValue("m_AssetId", 1.ToGuid().ToString("N"));

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

		private void InitPlayerName()
		{
			QSBCore.UnityEvents.RunWhen(PlayerData.IsLoaded, () =>
			{
				try
				{
					var titleScreenManager = FindObjectOfType<TitleScreenManager>();
					var profileManager = titleScreenManager._profileManager;
					if (profileManager.GetType().Name == "MSStoreProfileManager")
					{
						PlayerName = (string)profileManager.GetType().GetProperty("userDisplayName").GetValue(profileManager);
					}
					else
					{
						PlayerName = StandaloneProfileManager.SharedInstance.currentProfile.profileName;
					}
				}
				catch (Exception ex)
				{
					DebugLog.ToConsole($"Error - Exception when getting player name : {ex}", MessageType.Error);
					PlayerName = "Player";
				}
			});
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
			template.GetRequiredComponent<NetworkIdentity>().SetValue("m_AssetId", assetId.ToGuid().ToString("N"));
			template.AddComponent(transformSyncType);
			return template;
		}

		private void Update()
		{
			_lastTransportError = null;
		}

		private void ConfigureNetworkManager()
		{
			networkAddress = QSBCore.DefaultServerIP;
			maxConnections = MaxConnections;

			kcp2k.Log.Info = s => DebugLog.DebugWrite("[KCP] " + s);
			kcp2k.Log.Warning = s =>
			{
				DebugLog.DebugWrite("[KCP] " + s, MessageType.Warning);
				_lastTransportError = s;
			};
			kcp2k.Log.Error = s =>
			{
				DebugLog.DebugWrite("[KCP] " + s, MessageType.Error);
				_lastTransportError = s;
			};

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

		public override void OnServerAddPlayer(NetworkConnection connection) // Called on the server when a client joins
		{
			DebugLog.DebugWrite($"OnServerAddPlayer", MessageType.Info);
			base.OnServerAddPlayer(connection);

			NetworkServer.Spawn(Instantiate(_probePrefab), connection);
		}

		public override void OnStartClient()
		{
			var config = QSBCore.Helper.Config;
			config.SetSettingsValue("defaultServerIP", networkAddress);
			QSBCore.Helper.Storage.Save(config, Constants.ModConfigFileName);
		}

		public override void OnClientConnect() // Called on the client when connecting to a server
		{
			DebugLog.DebugWrite("OnClientConnect", MessageType.Info);
			base.OnClientConnect();

			OnClientConnected?.SafeInvoke();

			QSBMessageManager.Init();

			gameObject.AddComponent<RespawnOnDeath>();
			gameObject.AddComponent<ServerStateManager>();
			gameObject.AddComponent<ClientStateManager>();

			if (QSBSceneManager.IsInUniverse)
			{
				QSBWorldSync.BuildWorldObjects(QSBSceneManager.CurrentScene);
			}

			var specificType = QSBCore.IsHost ? QSBPatchTypes.OnServerClientConnect : QSBPatchTypes.OnNonServerClientConnect;
			QSBPatchManager.DoPatchType(specificType);
			QSBPatchManager.DoPatchType(QSBPatchTypes.OnClientConnect);

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

			QSBWorldSync.RemoveWorldObjects();

			if (WakeUpSync.LocalInstance != null)
			{
				WakeUpSync.LocalInstance.OnDisconnect();
			}

			if (_everConnected)
			{
				var specificType = QSBCore.IsHost ? QSBPatchTypes.OnServerClientConnect : QSBPatchTypes.OnNonServerClientConnect;
				QSBPatchManager.DoUnpatchType(specificType);
				QSBPatchManager.DoUnpatchType(QSBPatchTypes.OnClientConnect);
			}

			_everConnected = false;
		}

		public override void OnClientDisconnect()
		{
			base.OnClientDisconnect();
			if (_intentionalDisconnect)
			{
				_lastTransportError = null;
				_intentionalDisconnect = false;
			}
			else if (_lastTransportError == null)
			{
				_lastTransportError = "host disconnected";
			}

			OnClientDisconnected?.SafeInvoke(_lastTransportError);
		}

		public override void OnServerDisconnect(NetworkConnection conn) // Called on the server when any client disconnects
		{
			DebugLog.DebugWrite("OnServerDisconnect", MessageType.Info);

			// revert authority from ship
			if (ShipTransformSync.LocalInstance != null)
			{
				var identity = ShipTransformSync.LocalInstance.netIdentity;
				if (identity != null && identity.connectionToClient == conn)
				{
					identity.SetAuthority(QSBPlayerManager.LocalPlayerId);
				}
			}

			// stop dragging for the orbs this player was dragging
			foreach (var qsbOrb in QSBWorldSync.GetWorldObjects<QSBOrb>())
			{
				if (qsbOrb.TransformSync == null)
				{
					DebugLog.ToConsole($"{qsbOrb} TransformSync == null??????????", MessageType.Warning);
					continue;
				}

				var identity = qsbOrb.TransformSync.netIdentity;
				if (identity.connectionToClient == conn)
				{
					qsbOrb.SetDragging(false);
					qsbOrb.SendMessage(new OrbDragMessage(false));
				}
			}

			AuthorityManager.OnDisconnect(conn);

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
	}
}
