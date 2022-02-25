using Epic.OnlineServices.Logging;
using EpicTransport;
using Mirror;
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
using QSB.Syncs.Occasional;
using QSB.TimeSync;
using QSB.Tools.ProbeTool.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QSB;

public class QSBNetworkManager : NetworkManager, IAddComponentOnStart
{
	public static new QSBNetworkManager singleton => (QSBNetworkManager)NetworkManager.singleton;

	public event Action OnClientConnected;
	public event Action<string> OnClientDisconnected;

	public GameObject OrbPrefab { get; private set; }
	public GameObject ShipPrefab { get; private set; }
	public GameObject AnglerPrefab { get; private set; }
	public GameObject JellyfishPrefab { get; private set; }
	public GameObject OccasionalPrefab { get; private set; }
	private string PlayerName { get; set; }

	private const int MaxConnections = 128;

	private GameObject _probePrefab;
	private bool _everConnected;

	private string _lastTransportError;
	private static readonly string[] _kcpErrorLogs =
	{
		"KCP: received disconnect message",
		"Failed to resolve host: .*"
	};

	public override void Awake()
	{
		gameObject.SetActive(false);

		if (QSBCore.DebugSettings.UseKcpTransport)
		{
			transport = gameObject.AddComponent<kcp2k.KcpTransport>();
		}
		else
		{
			// https://dev.epicgames.com/portal/en-US/qsb/sdk/credentials/qsb
			var eosApiKey = ScriptableObject.CreateInstance<EosApiKey>();
			eosApiKey.epicProductName = "QSB";
			eosApiKey.epicProductVersion = "1.0";
			eosApiKey.epicProductId = "d4623220acb64419921c72047931b165";
			eosApiKey.epicSandboxId = "d9bc4035269747668524931b0840ca29";
			eosApiKey.epicDeploymentId = "1f164829371e4cdcb23efedce98d99ad";
			eosApiKey.epicClientId = "xyza7891TmlpkaiDv6KAnJH0f07aAbTu";
			eosApiKey.epicClientSecret = "ft17miukylHF877istFuhTgq+Kw1le3Pfigvf9Dtu20";

			var eosSdkComponent = gameObject.AddComponent<EOSSDKComponent>();
			eosSdkComponent.apiKeys = eosApiKey;
			eosSdkComponent.epicLoggerLevel = LogLevel.Info;
			eosSdkComponent.collectPlayerMetrics = false;

			var eosTransport = gameObject.AddComponent<EosTransport>();
			eosTransport.SetTransportError = error => _lastTransportError = error;
			transport = eosTransport;
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

	private void InitPlayerName() =>
		Delay.RunWhen(PlayerData.IsLoaded, () =>
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

			if (!QSBCore.DebugSettings.UseKcpTransport)
			{
				EOSSDKComponent.DisplayName = PlayerName;
			}
		});

	/// create a new network prefab from the network object prefab template.
	/// this works by calling Unload(false) and then reloading the AssetBundle,
	/// which makes LoadAsset give you a new resource.
	/// see https://docs.unity3d.com/Manual/AssetBundles-Native.html.
	private static GameObject MakeNewNetworkObject(int assetId, string name, Type transformSyncType)
	{
		var bundle = QSBCore.Helper.Assets.LoadBundle("AssetBundles/empty");
		var template = bundle.LoadAsset<GameObject>("Assets/Prefabs/Empty.prefab");
		bundle.Unload(false);

		DebugLog.DebugWrite($"MakeNewNetworkObject - prefab id {template.GetInstanceID()} "
		                    + $"for {assetId} {name} {transformSyncType.Name}");
		template.name = name;
		template.AddComponent<NetworkIdentity>().SetValue("m_AssetId", assetId.ToGuid().ToString("N"));
		template.AddComponent(transformSyncType);
		return template;
	}

	private void ConfigureNetworkManager()
	{
		networkAddress = QSBCore.DefaultServerIP;
		maxConnections = MaxConnections;

		if (QSBCore.DebugSettings.UseKcpTransport)
		{
			kcp2k.Log.Info = s =>
			{
				DebugLog.DebugWrite("[KCP] " + s);
				if (_kcpErrorLogs.Any(p => Regex.IsMatch(s, p)))
				{
					_lastTransportError = s;
				}
			};
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
		}

		DebugLog.DebugWrite("Network Manager ready.", MessageType.Success);
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient connection) // Called on the server when a client joins
	{
		DebugLog.DebugWrite($"OnServerAddPlayer", MessageType.Info);
		base.OnServerAddPlayer(connection);

		NetworkServer.Spawn(Instantiate(_probePrefab), connection);
	}

	public override void OnStartClient()
	{
		QSBCore.DefaultServerIP = networkAddress;
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
			QSBWorldSync.BuildWorldObjects(QSBSceneManager.CurrentScene).Forget();
		}

		var specificType = QSBCore.IsHost ? QSBPatchTypes.OnServerClientConnect : QSBPatchTypes.OnNonServerClientConnect;
		QSBPatchManager.DoPatchType(specificType);
		QSBPatchManager.DoPatchType(QSBPatchTypes.OnClientConnect);

		Delay.RunWhen(() => PlayerTransformSync.LocalInstance,
			() => new PlayerJoinMessage(PlayerName).Send());

		if (!QSBCore.IsHost)
		{
			Delay.RunWhen(() => PlayerTransformSync.LocalInstance,
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
		DebugLog.DebugWrite("OnClientDisconnect");
		base.OnClientDisconnect();
		OnClientDisconnected?.SafeInvoke(_lastTransportError);
		_lastTransportError = null;
	}

	public override void OnServerDisconnect(NetworkConnectionToClient conn) // Called on the server when any client disconnects
	{
		DebugLog.DebugWrite("OnServerDisconnect", MessageType.Info);

		// local conn = we are host, so skip
		if (conn is not LocalConnectionToClient)
		{
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
		}

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