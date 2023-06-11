using Epic.OnlineServices.Logging;
using Mirror;
using Mirror.FizzySteam;
using OWML.Common;
using OWML.Utils;
using QSB.Anglerfish.TransformSync;
using QSB.ClientServerStateSync;
using QSB.DeathSync;
using QSB.EchoesOfTheEye.AirlockSync.VariableSync;
using QSB.EchoesOfTheEye.EclipseDoors.VariableSync;
using QSB.EchoesOfTheEye.EclipseElevators.VariableSync;
using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.JellyfishSync.TransformSync;
using QSB.Menus;
using QSB.Messaging;
using QSB.ModelShip;
using QSB.ModelShip.TransformSync;
using QSB.OrbSync.Messages;
using QSB.OrbSync.TransformSync;
using QSB.OrbSync.WorldObjects;
using QSB.OwnershipSync;
using QSB.Patches;
using QSB.Player;
using QSB.Player.Messages;
using QSB.Player.TransformSync;
using QSB.SaveSync;
using QSB.ShipSync;
using QSB.ShipSync.TransformSync;
using QSB.Syncs.Occasional;
using QSB.TimeSync;
using QSB.Tools.ProbeLauncherTool.VariableSync;
using QSB.Tools.ProbeTool.TransformSync;
using QSB.Utility;
using QSB.Utility.VariableSync;
using QSB.WorldSync;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QSB;

public class QSBNetworkManager : NetworkManager, IAddComponentOnStart
{
	public new static QSBNetworkManager singleton => (QSBNetworkManager)NetworkManager.singleton;

	public event Action OnClientConnected;
	public event Action<TransportError, string> OnClientDisconnected;

	public GameObject OrbPrefab { get; private set; }
	public GameObject ShipPrefab { get; private set; }
	public GameObject AnglerPrefab { get; private set; }
	public GameObject JellyfishPrefab { get; private set; }
	public GameObject OccasionalPrefab { get; private set; }
	public GameObject RaftPrefab { get; private set; }
	public GameObject DoorPrefab { get; private set; }
	public GameObject ElevatorPrefab { get; private set; }
	public GameObject AirlockPrefab { get; private set; }
	public GameObject ShipModulePrefab { get; private set; }
	public GameObject ShipLegPrefab { get; private set; }
	public GameObject ModelShipPrefab { get; private set; }
	public GameObject StationaryProbeLauncherPrefab { get; private set; }
	private string PlayerName { get; set; }

	private GameObject _probePrefab;
	private bool _everConnected;

	private (TransportError error, string reason) _lastTransportError = (TransportError.Unexpected, "transport did not give an error. uh oh");

	private static kcp2k.KcpTransport _kcpTransport;
	private static FizzySteamworks _steamTransport;

	public override void Awake()
	{
		gameObject.SetActive(false);

		{
			_kcpTransport = gameObject.AddComponent<kcp2k.KcpTransport>();
		}

		{
			_steamTransport = gameObject.AddComponent<FizzySteamworks>();
		}

		transport = QSBCore.UseKcpTransport ? _kcpTransport : _steamTransport;

		gameObject.SetActive(true);

		base.Awake();

		InitPlayerName();
		QSBCore.ProfileManager.OnProfileSignInComplete += _ => InitPlayerName();

		playerPrefab = QSBCore.NetworkAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/NETWORK_Player_Body.prefab");
		playerPrefab.GetRequiredComponent<NetworkIdentity>().SetValue("_assetId", (uint)1);

		ShipPrefab = MakeNewNetworkObject(2, "NetworkShip", typeof(ShipTransformSync));
		var shipVector3Sync = ShipPrefab.AddComponent<Vector3VariableSyncer>();
		var shipThrustSync = ShipPrefab.AddComponent<ShipThrusterVariableSyncer>();
		shipThrustSync.AccelerationSyncer = shipVector3Sync;
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

		RaftPrefab = MakeNewNetworkObject(8, "NetworkRaft", typeof(RaftTransformSync));
		spawnPrefabs.Add(RaftPrefab);

		DoorPrefab = MakeNewNetworkObject(9, "NetworkEclipseDoor", typeof(EclipseDoorVariableSyncer));
		spawnPrefabs.Add(DoorPrefab);

		ElevatorPrefab = MakeNewNetworkObject(10, "NetworkEclipseElevator", typeof(EclipseElevatorVariableSyncer));
		spawnPrefabs.Add(ElevatorPrefab);

		AirlockPrefab = MakeNewNetworkObject(11, "NetworkGhostAirlock", typeof(AirlockVariableSyncer));
		spawnPrefabs.Add(AirlockPrefab);

		ShipModulePrefab = MakeNewNetworkObject(12, "NetworkShipModule", typeof(ShipModuleTransformSync));
		spawnPrefabs.Add(ShipModulePrefab);

		ShipLegPrefab = MakeNewNetworkObject(13, "NetworkShipLeg", typeof(ShipLegTransformSync));
		spawnPrefabs.Add(ShipLegPrefab);

		ModelShipPrefab = MakeNewNetworkObject(14, "NetworkModelShip", typeof(ModelShipTransformSync));
		var modelShipVector3Syncer = ModelShipPrefab.AddComponent<Vector3VariableSyncer>();
		var modelShipThrusterVariableSyncer = ModelShipPrefab.AddComponent<ModelShipThrusterVariableSyncer>();
		modelShipThrusterVariableSyncer.AccelerationSyncer = modelShipVector3Syncer;
		spawnPrefabs.Add(ModelShipPrefab);

		StationaryProbeLauncherPrefab = MakeNewNetworkObject(15, "NetworkStationaryProbeLauncher", typeof(StationaryProbeLauncherVariableSyncer));
		spawnPrefabs.Add(StationaryProbeLauncherPrefab);

		ConfigureNetworkManager();
	}

	public static void UpdateTransport()
	{
		if (QSBCore.IsInMultiplayer)
		{
			return;
		}
		if (singleton != null)
		{
			singleton.transport = Transport.active = QSBCore.UseKcpTransport ? _kcpTransport : _steamTransport;
		}
		if (MenuManager.Instance != null)
		{
			MenuManager.Instance.OnLanguageChanged(); // hack to update text
		}
	}

	private void InitPlayerName() =>
		Delay.RunWhen(PlayerData.IsLoaded, () =>
		{
			try
			{
				if (!QSBCore.IsStandalone)
				{
					PlayerName = QSBMSStoreProfileManager.SharedInstance.userDisplayName;
				}
				else
				{
					var currentProfile = QSBStandaloneProfileManager.SharedInstance.currentProfile;

					if (currentProfile == null)
					{
						// probably havent created a profile yet
						Delay.RunWhen(() => QSBStandaloneProfileManager.SharedInstance.currentProfile != null, () => InitPlayerName());
						return;
					}

					PlayerName = currentProfile.profileName;
				}
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error - Exception when getting player name : {ex}", MessageType.Error);
				PlayerName = "Player";
			}
		});

	/// create a new network prefab from the network object prefab template.
	/// this works by calling Unload(false) and then reloading the AssetBundle,
	/// which makes LoadAsset give you a new resource.
	/// see https://docs.unity3d.com/Manual/AssetBundles-Native.html.
	private static GameObject MakeNewNetworkObject(uint assetId, string name, Type networkBehaviourType)
	{
		var bundle = QSBCore.Helper.Assets.LoadBundle("AssetBundles/qsb_empty");

		if (bundle == null)
		{
			DebugLog.ToConsole($"FATAL - An assetbundle is missing! Re-install mod or contact devs.", MessageType.Fatal);
			return null;
		}

		var template = bundle.LoadAsset<GameObject>("Assets/Prefabs/Empty.prefab");
		bundle.Unload(false);

		template.name = name;
		template.AddComponent<NetworkIdentity>().SetValue("_assetId", assetId);
		template.AddComponent(networkBehaviourType);
		return template;
	}

	private void ConfigureNetworkManager()
	{
		networkAddress = QSBCore.DefaultServerIP;

		{
			kcp2k.Log.Info = s =>
			{
				DebugLog.DebugWrite("[KCP] " + s);
				// hack
				if (s == "KcpPeer: received disconnect message")
				{
					OnClientError(TransportError.ConnectionClosed, s);
				}
			};
			kcp2k.Log.Warning = s => DebugLog.DebugWrite("[KCP] " + s, MessageType.Warning);
			kcp2k.Log.Error = s => DebugLog.DebugWrite("[KCP] " + s, MessageType.Error);
		}

		QSBSceneManager.OnPostSceneLoad += (_, loadScene) =>
		{
			if (QSBCore.IsInMultiplayer && loadScene == OWScene.TitleScreen)
			{
				StopHost();
			}
		};

		DebugLog.DebugWrite("Network Manager ready.", MessageType.Success);
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient connection) // Called on the server when a client joins
	{
		DebugLog.DebugWrite("OnServerAddPlayer", MessageType.Info);
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
		QSBPlayerManager.PlayerList.ForEach(player =>
		{
			player.HudMarker?.Remove();
			player.MapMarker?.Remove();
		});

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
		OnClientDisconnected?.SafeInvoke(_lastTransportError.error, _lastTransportError.reason);
		_lastTransportError = (TransportError.Unexpected, "transport did not give an error. uh oh");
	}

	public override void OnServerDisconnect(NetworkConnectionToClient conn) // Called on the server when any client disconnects
	{
		DebugLog.DebugWrite("OnServerDisconnect", MessageType.Info);

		// local conn = we are host, so skip
		if (conn is not LocalConnectionToClient)
		{
			// revert ownership from ship
			if (ShipTransformSync.LocalInstance != null)
			{
				var identity = ShipTransformSync.LocalInstance.netIdentity;
				if (identity != null && identity.connectionToClient == conn)
				{
					identity.SetOwner(QSBPlayerManager.LocalPlayerId);
				}
			}
			// revert ownership from model ship
			if (ModelShipTransformSync.LocalInstance != null)
			{
				var identity = ModelShipTransformSync.LocalInstance.netIdentity;
				if (identity != null && identity.connectionToClient == conn)
				{
					identity.SetOwner(QSBPlayerManager.LocalPlayerId);
				}
			}

			// stop dragging for the orbs this player was dragging
			// i THINK this is here because orb ownership is in network behavior, which may not work properly in OnPlayerLeave
			foreach (var qsbOrb in QSBWorldSync.GetWorldObjects<QSBOrb>())
			{
				if (qsbOrb.NetworkBehaviour == null)
				{
					DebugLog.ToConsole($"{qsbOrb} TransformSync == null??????????", MessageType.Warning);
					continue;
				}

				var identity = qsbOrb.NetworkBehaviour.netIdentity;
				if (identity.connectionToClient == conn)
				{
					qsbOrb.SetDragging(false);
					qsbOrb.SendMessage(new OrbDragMessage(false));
				}
			}

			OwnershipManager.OnDisconnect(conn);
		}

		base.OnServerDisconnect(conn);
	}

	public override void OnStopServer()
	{
		DebugLog.DebugWrite("OnStopServer", MessageType.Info);
		Destroy(GetComponent<RespawnOnDeath>());
		DebugLog.ToConsole("Server stopped!", MessageType.Info);
		QSBPlayerManager.PlayerList.ForEach(player =>
		{
			player.HudMarker?.Remove();
			player.MapMarker?.Remove();
		});

		base.OnStopServer();
	}

	public override void OnServerError(NetworkConnectionToClient conn, TransportError error, string reason)
	{
		DebugLog.DebugWrite($"OnServerError({conn}, {error}, {reason})", MessageType.Error);
		_lastTransportError = (error, reason);
	}

	public override void OnClientError(TransportError error, string reason)
	{
		DebugLog.DebugWrite($"OnClientError({error}, {reason})", MessageType.Error);
		_lastTransportError = (error, reason);
	}
}
