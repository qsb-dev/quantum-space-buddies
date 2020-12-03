using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

namespace QSB.QuantumUNET
{
	public class QSBNetworkManagerUNET : MonoBehaviour
	{
		public int networkPort
		{
			get
			{
				return m_NetworkPort;
			}
			set
			{
				m_NetworkPort = value;
			}
		}

		public bool serverBindToIP
		{
			get
			{
				return m_ServerBindToIP;
			}
			set
			{
				m_ServerBindToIP = value;
			}
		}

		public string serverBindAddress
		{
			get
			{
				return m_ServerBindAddress;
			}
			set
			{
				m_ServerBindAddress = value;
			}
		}

		public string networkAddress
		{
			get
			{
				return m_NetworkAddress;
			}
			set
			{
				m_NetworkAddress = value;
			}
		}

		public bool dontDestroyOnLoad
		{
			get
			{
				return m_DontDestroyOnLoad;
			}
			set
			{
				m_DontDestroyOnLoad = value;
			}
		}

		public bool runInBackground
		{
			get
			{
				return m_RunInBackground;
			}
			set
			{
				m_RunInBackground = value;
			}
		}

		public bool scriptCRCCheck
		{
			get
			{
				return m_ScriptCRCCheck;
			}
			set
			{
				m_ScriptCRCCheck = value;
			}
		}

		[Obsolete("moved to NetworkMigrationManager")]
		public bool sendPeerInfo
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public float maxDelay
		{
			get
			{
				return m_MaxDelay;
			}
			set
			{
				m_MaxDelay = value;
			}
		}

		public LogFilter.FilterLevel logLevel
		{
			get
			{
				return m_LogLevel;
			}
			set
			{
				m_LogLevel = value;
				LogFilter.currentLogLevel = (int)value;
			}
		}

		public GameObject playerPrefab
		{
			get
			{
				return m_PlayerPrefab;
			}
			set
			{
				DebugLog.DebugWrite("setting player prefab");
				m_PlayerPrefab = value;
			}
		}

		public bool autoCreatePlayer
		{
			get
			{
				return m_AutoCreatePlayer;
			}
			set
			{
				m_AutoCreatePlayer = value;
			}
		}

		public PlayerSpawnMethod playerSpawnMethod
		{
			get
			{
				return m_PlayerSpawnMethod;
			}
			set
			{
				m_PlayerSpawnMethod = value;
			}
		}

		public string offlineScene
		{
			get
			{
				return m_OfflineScene;
			}
			set
			{
				m_OfflineScene = value;
			}
		}

		public string onlineScene
		{
			get
			{
				return m_OnlineScene;
			}
			set
			{
				m_OnlineScene = value;
			}
		}

		public List<GameObject> spawnPrefabs
		{
			get
			{
				return m_SpawnPrefabs;
			}
		}

		public List<Transform> startPositions
		{
			get
			{
				return s_StartPositions;
			}
		}

		public bool customConfig
		{
			get
			{
				return m_CustomConfig;
			}
			set
			{
				m_CustomConfig = value;
			}
		}

		public ConnectionConfig connectionConfig
		{
			get
			{
				if (m_ConnectionConfig == null)
				{
					m_ConnectionConfig = new ConnectionConfig();
				}
				return m_ConnectionConfig;
			}
		}

		public GlobalConfig globalConfig
		{
			get
			{
				if (m_GlobalConfig == null)
				{
					m_GlobalConfig = new GlobalConfig();
				}
				return m_GlobalConfig;
			}
		}

		public int maxConnections
		{
			get
			{
				return m_MaxConnections;
			}
			set
			{
				m_MaxConnections = value;
			}
		}

		public List<QosType> channels
		{
			get
			{
				return m_Channels;
			}
		}

		public EndPoint secureTunnelEndpoint
		{
			get
			{
				return m_EndPoint;
			}
			set
			{
				m_EndPoint = value;
			}
		}

		public bool useWebSockets
		{
			get
			{
				return m_UseWebSockets;
			}
			set
			{
				m_UseWebSockets = value;
			}
		}

		public bool useSimulator
		{
			get
			{
				return m_UseSimulator;
			}
			set
			{
				m_UseSimulator = value;
			}
		}

		public int simulatedLatency
		{
			get
			{
				return m_SimulatedLatency;
			}
			set
			{
				m_SimulatedLatency = value;
			}
		}

		public float packetLossPercentage
		{
			get
			{
				return m_PacketLossPercentage;
			}
			set
			{
				m_PacketLossPercentage = value;
			}
		}

		public string matchHost
		{
			get
			{
				return m_MatchHost;
			}
			set
			{
				m_MatchHost = value;
			}
		}

		public int matchPort
		{
			get
			{
				return m_MatchPort;
			}
			set
			{
				m_MatchPort = value;
			}
		}

		public bool clientLoadedScene
		{
			get
			{
				return m_ClientLoadedScene;
			}
			set
			{
				m_ClientLoadedScene = value;
			}
		}

		public QSBNetworkMigrationManager migrationManager
		{
			get
			{
				return m_MigrationManager;
			}
		}

		public int numPlayers
		{
			get
			{
				int num = 0;
				for (int i = 0; i < QSBNetworkServer.connections.Count; i++)
				{
					QSBNetworkConnection networkConnection = QSBNetworkServer.connections[i];
					if (networkConnection != null)
					{
						for (int j = 0; j < networkConnection.PlayerControllers.Count; j++)
						{
							if (networkConnection.PlayerControllers[j].IsValid)
							{
								num++;
							}
						}
					}
				}
				return num;
			}
		}

		private void Awake()
		{
			InitializeSingleton();
		}

		private void InitializeSingleton()
		{
			if (!(singleton != null) || !(singleton == this))
			{
				int logLevel = (int)m_LogLevel;
				if (logLevel != -1)
				{
					LogFilter.currentLogLevel = logLevel;
				}
				if (m_DontDestroyOnLoad)
				{
					if (singleton != null)
					{
						Debug.Log("Multiple NetworkManagers detected in the scene. Only one NetworkManager can exist at a time. The duplicate NetworkManager will not be used.");
						Destroy(base.gameObject);
						return;
					}
					Debug.Log("NetworkManager created singleton (DontDestroyOnLoad)");
					singleton = this;
					if (Application.isPlaying)
					{
						DontDestroyOnLoad(base.gameObject);
					}
				}
				else
				{
					Debug.Log("NetworkManager created singleton (ForScene)");
					singleton = this;
				}
				if (m_NetworkAddress != "")
				{
					s_Address = m_NetworkAddress;
				}
				else if (s_Address != "")
				{
					m_NetworkAddress = s_Address;
				}
			}
		}

		private void OnValidate()
		{
			if (m_SimulatedLatency < 1)
			{
				m_SimulatedLatency = 1;
			}
			if (m_SimulatedLatency > 500)
			{
				m_SimulatedLatency = 500;
			}
			if (m_PacketLossPercentage < 0f)
			{
				m_PacketLossPercentage = 0f;
			}
			if (m_PacketLossPercentage > 99f)
			{
				m_PacketLossPercentage = 99f;
			}
			if (m_MaxConnections <= 0)
			{
				m_MaxConnections = 1;
			}
			if (m_MaxConnections > 32000)
			{
				m_MaxConnections = 32000;
			}
			if (m_MaxBufferedPackets <= 0)
			{
				m_MaxBufferedPackets = 0;
			}
			if (m_MaxBufferedPackets > 512)
			{
				m_MaxBufferedPackets = 512;
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkManager - MaxBufferedPackets cannot be more than " + 512);
				}
			}
			if (m_PlayerPrefab != null && m_PlayerPrefab.GetComponent<NetworkIdentity>() == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkManager - playerPrefab must have a NetworkIdentity.");
				}
				m_PlayerPrefab = null;
			}
			if (m_ConnectionConfig != null && m_ConnectionConfig.MinUpdateTimeout <= 0U)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkManager MinUpdateTimeout cannot be zero or less. The value will be reset to 1 millisecond");
				}
				m_ConnectionConfig.MinUpdateTimeout = 1U;
			}
			if (m_GlobalConfig != null)
			{
				if (m_GlobalConfig.ThreadAwakeTimeout <= 0U)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("NetworkManager ThreadAwakeTimeout cannot be zero or less. The value will be reset to 1 millisecond");
					}
					m_GlobalConfig.ThreadAwakeTimeout = 1U;
				}
			}
		}

		internal void RegisterServerMessages()
		{
			QSBNetworkServer.RegisterHandler(32, new QSBNetworkMessageDelegate(OnServerConnectInternal));
			QSBNetworkServer.RegisterHandler(33, new QSBNetworkMessageDelegate(OnServerDisconnectInternal));
			QSBNetworkServer.RegisterHandler(35, new QSBNetworkMessageDelegate(OnServerReadyMessageInternal));
			QSBNetworkServer.RegisterHandler(37, new QSBNetworkMessageDelegate(OnServerAddPlayerMessageInternal));
			QSBNetworkServer.RegisterHandler(38, new QSBNetworkMessageDelegate(OnServerRemovePlayerMessageInternal));
			QSBNetworkServer.RegisterHandler(34, new QSBNetworkMessageDelegate(OnServerErrorInternal));
		}

		public void SetupMigrationManager(QSBNetworkMigrationManager man)
		{
			m_MigrationManager = man;
		}

		public bool StartServer(ConnectionConfig config, int maxConnections)
		{
			return StartServer(null, config, maxConnections);
		}

		public bool StartServer()
		{
			return StartServer(null);
		}

		public bool StartServer(MatchInfo info)
		{
			return StartServer(info, null, -1);
		}

		private bool StartServer(MatchInfo info, ConnectionConfig config, int maxConnections)
		{
			InitializeSingleton();
			OnStartServer();
			if (m_RunInBackground)
			{
				Application.runInBackground = true;
			}
			QSBNetworkCRC.scriptCRCCheck = scriptCRCCheck;
			QSBNetworkServer.useWebSockets = m_UseWebSockets;
			if (m_GlobalConfig != null)
			{
				NetworkTransport.Init(m_GlobalConfig);
			}
			if (m_CustomConfig && m_ConnectionConfig != null && config == null)
			{
				m_ConnectionConfig.Channels.Clear();
				for (int i = 0; i < m_Channels.Count; i++)
				{
					m_ConnectionConfig.AddChannel(m_Channels[i]);
				}
				QSBNetworkServer.Configure(m_ConnectionConfig, m_MaxConnections);
			}
			if (config != null)
			{
				QSBNetworkServer.Configure(config, maxConnections);
			}
			if (info != null)
			{
				if (!QSBNetworkServer.Listen(info, m_NetworkPort))
				{
					if (LogFilter.logError)
					{
						Debug.LogError("StartServer listen failed.");
					}
					return false;
				}
			}
			else if (m_ServerBindToIP && !string.IsNullOrEmpty(m_ServerBindAddress))
			{
				if (!QSBNetworkServer.Listen(m_ServerBindAddress, m_NetworkPort))
				{
					if (LogFilter.logError)
					{
						Debug.LogError("StartServer listen on " + m_ServerBindAddress + " failed.");
					}
					return false;
				}
			}
			else if (!QSBNetworkServer.Listen(m_NetworkPort))
			{
				if (LogFilter.logError)
				{
					Debug.LogError("StartServer listen failed.");
				}
				return false;
			}
			RegisterServerMessages();
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager StartServer port:" + m_NetworkPort);
			}
			isNetworkActive = true;
			string name = SceneManager.GetSceneAt(0).name;
			if (!string.IsNullOrEmpty(m_OnlineScene) && m_OnlineScene != name && m_OnlineScene != m_OfflineScene)
			{
				ServerChangeScene(m_OnlineScene);
			}
			else
			{
				QSBNetworkServer.SpawnObjects();
			}
			return true;
		}

		internal void RegisterClientMessages(QSBNetworkClient client)
		{
			client.RegisterHandler(32, new QSBNetworkMessageDelegate(OnClientConnectInternal));
			client.RegisterHandler(33, new QSBNetworkMessageDelegate(OnClientDisconnectInternal));
			client.RegisterHandler(36, new QSBNetworkMessageDelegate(OnClientNotReadyMessageInternal));
			client.RegisterHandler(34, new QSBNetworkMessageDelegate(OnClientErrorInternal));
			client.RegisterHandler(39, new QSBNetworkMessageDelegate(OnClientSceneInternal));
			if (m_PlayerPrefab != null)
			{
				QSBClientScene.RegisterPrefab(m_PlayerPrefab);
			}
			for (int i = 0; i < m_SpawnPrefabs.Count; i++)
			{
				GameObject gameObject = m_SpawnPrefabs[i];
				if (gameObject != null)
				{
					QSBClientScene.RegisterPrefab(gameObject);
				}
			}
		}

		public void UseExternalClient(QSBNetworkClient externalClient)
		{
			if (m_RunInBackground)
			{
				Application.runInBackground = true;
			}
			if (externalClient != null)
			{
				client = externalClient;
				isNetworkActive = true;
				this.RegisterClientMessages(client);
				this.OnStartClient(client);
			}
			else
			{
				OnStopClient();
				QSBClientScene.DestroyAllClientObjects();
				QSBClientScene.HandleClientDisconnect(client.connection);
				client = null;
				if (!string.IsNullOrEmpty(m_OfflineScene))
				{
					ClientChangeScene(m_OfflineScene, false);
				}
			}
			s_Address = m_NetworkAddress;
		}

		public QSBNetworkClient StartClient(MatchInfo info, ConnectionConfig config, int hostPort)
		{
			DebugLog.DebugWrite("start client proper");
			InitializeSingleton();
			matchInfo = info;
			if (m_RunInBackground)
			{
				Application.runInBackground = true;
			}
			isNetworkActive = true;
			if (m_GlobalConfig != null)
			{
				NetworkTransport.Init(m_GlobalConfig);
			}
			client = new QSBNetworkClient();
			client.hostPort = hostPort;
			if (config != null)
			{
				if (config.UsePlatformSpecificProtocols && Application.platform != RuntimePlatform.PS4 && Application.platform != RuntimePlatform.PSP2)
				{
					throw new ArgumentOutOfRangeException("Platform specific protocols are not supported on this platform");
				}
				client.Configure(config, 1);
			}
			else if (m_CustomConfig && m_ConnectionConfig != null)
			{
				m_ConnectionConfig.Channels.Clear();
				for (int i = 0; i < m_Channels.Count; i++)
				{
					m_ConnectionConfig.AddChannel(m_Channels[i]);
				}
				if (m_ConnectionConfig.UsePlatformSpecificProtocols && Application.platform != RuntimePlatform.PS4 && Application.platform != RuntimePlatform.PSP2)
				{
					throw new ArgumentOutOfRangeException("Platform specific protocols are not supported on this platform");
				}
				client.Configure(m_ConnectionConfig, m_MaxConnections);
			}
			this.RegisterClientMessages(client);
			if (matchInfo != null)
			{
				DebugLog.DebugWrite("NetworkManager StartClient match: " + matchInfo);
				client.Connect(matchInfo);
			}
			else if (m_EndPoint != null)
			{
				DebugLog.DebugWrite("NetworkManager StartClient using provided SecureTunnel");
				client.Connect(m_EndPoint);
			}
			else
			{
				if (string.IsNullOrEmpty(m_NetworkAddress))
				{
					DebugLog.DebugWrite("Must set the Network Address field in the manager");
					return null;
				}
				DebugLog.DebugWrite(string.Concat(new object[]
				{
					"NetworkManager StartClient address:",
					m_NetworkAddress,
					" port:",
					m_NetworkPort
				}));
				if (m_UseSimulator)
				{
					DebugLog.DebugWrite("connecting with simulator");
					client.ConnectWithSimulator(m_NetworkAddress, m_NetworkPort, m_SimulatedLatency, m_PacketLossPercentage);
				}
				else
				{
					DebugLog.DebugWrite("connecting");
					client.Connect(m_NetworkAddress, m_NetworkPort);
				}
			}
			if (m_MigrationManager != null)
			{
				m_MigrationManager.Initialize(client, matchInfo);
			}
			this.OnStartClient(client);
			s_Address = m_NetworkAddress;
			return client;
		}

		public QSBNetworkClient StartClient(MatchInfo matchInfo)
		{
			return StartClient(matchInfo, null);
		}

		public QSBNetworkClient StartClient()
		{
			DebugLog.DebugWrite("start client");
			return StartClient(null, null);
		}

		public QSBNetworkClient StartClient(MatchInfo info, ConnectionConfig config)
		{
			return StartClient(info, config, 0);
		}

		public virtual QSBNetworkClient StartHost(ConnectionConfig config, int maxConnections)
		{
			OnStartHost();
			QSBNetworkClient result;
			if (StartServer(config, maxConnections))
			{
				QSBNetworkClient networkClient = ConnectLocalClient();
				OnServerConnect(networkClient.connection);
				OnStartClient(networkClient);
				result = networkClient;
			}
			else
			{
				result = null;
			}
			return result;
		}

		public virtual QSBNetworkClient StartHost(MatchInfo info)
		{
			OnStartHost();
			matchInfo = info;
			QSBNetworkClient result;
			if (StartServer(info))
			{
				QSBNetworkClient networkClient = ConnectLocalClient();
				OnStartClient(networkClient);
				result = networkClient;
			}
			else
			{
				result = null;
			}
			return result;
		}

		public virtual QSBNetworkClient StartHost()
		{
			OnStartHost();
			QSBNetworkClient result;
			if (StartServer())
			{
				QSBNetworkClient networkClient = ConnectLocalClient();
				OnStartClient(networkClient);
				result = networkClient;
			}
			else
			{
				result = null;
			}
			return result;
		}

		private QSBNetworkClient ConnectLocalClient()
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager StartHost port:" + m_NetworkPort);
			}
			m_NetworkAddress = "localhost";
			client = QSBClientScene.ConnectLocalServer();
			this.RegisterClientMessages(client);
			if (m_MigrationManager != null)
			{
				m_MigrationManager.Initialize(client, matchInfo);
			}
			return client;
		}

		public void StopHost()
		{
			bool active = QSBNetworkServer.active;
			OnStopHost();
			StopServer();
			StopClient();
			if (m_MigrationManager != null)
			{
				if (active)
				{
					m_MigrationManager.LostHostOnHost();
				}
			}
		}

		public void StopServer()
		{
			if (QSBNetworkServer.active)
			{
				OnStopServer();
				if (LogFilter.logDebug)
				{
					Debug.Log("NetworkManager StopServer");
				}
				isNetworkActive = false;
				QSBNetworkServer.Shutdown();
				StopMatchMaker();
				if (!string.IsNullOrEmpty(m_OfflineScene))
				{
					ServerChangeScene(m_OfflineScene);
				}
				CleanupNetworkIdentities();
			}
		}

		public void StopClient()
		{
			OnStopClient();
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager StopClient");
			}
			isNetworkActive = false;
			if (client != null)
			{
				client.Disconnect();
				client.Shutdown();
				client = null;
			}
			StopMatchMaker();
			ClientScene.DestroyAllClientObjects();
			if (!string.IsNullOrEmpty(m_OfflineScene))
			{
				ClientChangeScene(m_OfflineScene, false);
			}
			CleanupNetworkIdentities();
		}

		public virtual void ServerChangeScene(string newSceneName)
		{
			if (string.IsNullOrEmpty(newSceneName))
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ServerChangeScene empty scene name");
				}
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("ServerChangeScene " + newSceneName);
				}
				QSBNetworkServer.SetAllClientsNotReady();
				networkSceneName = newSceneName;
				s_LoadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
				StringMessage msg = new StringMessage(networkSceneName);
				QSBNetworkServer.SendToAll(39, msg);
				s_StartPositionIndex = 0;
				s_StartPositions.Clear();
			}
		}

		private void CleanupNetworkIdentities()
		{
			foreach (QSBNetworkIdentity networkIdentity in Resources.FindObjectsOfTypeAll<QSBNetworkIdentity>())
			{
				networkIdentity.MarkForReset();
			}
		}

		internal void ClientChangeScene(string newSceneName, bool forceReload)
		{
			if (string.IsNullOrEmpty(newSceneName))
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ClientChangeScene empty scene name");
				}
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("ClientChangeScene newSceneName:" + newSceneName + " networkSceneName:" + networkSceneName);
				}
				if (newSceneName == networkSceneName)
				{
					if (m_MigrationManager != null)
					{
						FinishLoadScene();
						return;
					}
					if (!forceReload)
					{
						FinishLoadScene();
						return;
					}
				}
				s_LoadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName);
				networkSceneName = newSceneName;
			}
		}

		private void FinishLoadScene()
		{
			if (client != null)
			{
				if (s_ClientReadyConnection != null)
				{
					m_ClientLoadedScene = true;
					this.OnClientConnect(s_ClientReadyConnection);
					s_ClientReadyConnection = null;
				}
			}
			else
			{
				Debug.Log("FinishLoadScene client is null");
			}
			if (QSBNetworkServer.active)
			{
				QSBNetworkServer.SpawnObjects();
				OnServerSceneChanged(networkSceneName);
			}
			if (IsClientConnected() && client != null)
			{
				this.RegisterClientMessages(client);
				this.OnClientSceneChanged(client.connection);
			}
		}

		internal static void UpdateScene()
		{
			if (!(singleton == null))
			{
				if (s_LoadingSceneAsync != null)
				{
					if (s_LoadingSceneAsync.isDone)
					{
						DebugLog.DebugWrite("ClientChangeScene done readyCon:" + s_ClientReadyConnection);
						singleton.FinishLoadScene();
						s_LoadingSceneAsync.allowSceneActivation = true;
						s_LoadingSceneAsync = null;
					}
				}
			}
		}

		private void OnDestroy()
		{
			Debug.Log("NetworkManager destroyed");
		}

		public static void RegisterStartPosition(Transform start)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"RegisterStartPosition: (",
					start.gameObject.name,
					") ",
					start.position
				}));
			}
			s_StartPositions.Add(start);
		}

		public static void UnRegisterStartPosition(Transform start)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"UnRegisterStartPosition: (",
					start.gameObject.name,
					") ",
					start.position
				}));
			}
			s_StartPositions.Remove(start);
		}

		public bool IsClientConnected()
		{
			return client != null && client.isConnected;
		}

		public static void Shutdown()
		{
			if (!(singleton == null))
			{
				s_StartPositions.Clear();
				s_StartPositionIndex = 0;
				s_ClientReadyConnection = null;
				singleton.StopHost();
				singleton = null;
			}
		}

		internal void OnServerConnectInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerConnectInternal");
			}
			netMsg.conn.SetMaxDelay(m_MaxDelay);
			if (m_MaxBufferedPackets != 512)
			{
				for (int i = 0; i < QSBNetworkServer.numChannels; i++)
				{
					netMsg.conn.SetChannelOption(i, ChannelOption.MaxPendingBuffers, m_MaxBufferedPackets);
				}
			}
			if (!m_AllowFragmentation)
			{
				for (int j = 0; j < QSBNetworkServer.numChannels; j++)
				{
					netMsg.conn.SetChannelOption(j, ChannelOption.AllowFragmentation, 0);
				}
			}
			if (networkSceneName != "" && networkSceneName != m_OfflineScene)
			{
				StringMessage msg = new StringMessage(networkSceneName);
				netMsg.conn.Send(39, msg);
			}
			if (m_MigrationManager != null)
			{
				m_MigrationManager.SendPeerInfo();
			}
			OnServerConnect(netMsg.conn);
		}

		internal void OnServerDisconnectInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerDisconnectInternal");
			}
			if (m_MigrationManager != null)
			{
				m_MigrationManager.SendPeerInfo();
			}
			OnServerDisconnect(netMsg.conn);
		}

		internal void OnServerReadyMessageInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerReadyMessageInternal");
			}
			OnServerReady(netMsg.conn);
		}

		internal void OnServerAddPlayerMessageInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerAddPlayerMessageInternal");
			}
			netMsg.ReadMessage<AddPlayerMessage>(s_AddPlayerMessage);
			if (s_AddPlayerMessage.msgSize != 0)
			{
				NetworkReader extraMessageReader = new NetworkReader(s_AddPlayerMessage.msgData);
				this.OnServerAddPlayer(netMsg.conn, s_AddPlayerMessage.playerControllerId, extraMessageReader);
			}
			else
			{
				this.OnServerAddPlayer(netMsg.conn, s_AddPlayerMessage.playerControllerId);
			}
			if (m_MigrationManager != null)
			{
				m_MigrationManager.SendPeerInfo();
			}
		}

		internal void OnServerRemovePlayerMessageInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerRemovePlayerMessageInternal");
			}
			netMsg.ReadMessage<RemovePlayerMessage>(s_RemovePlayerMessage);
			QSBPlayerController player;
			netMsg.conn.GetPlayerController(s_RemovePlayerMessage.playerControllerId, out player);
			OnServerRemovePlayer(netMsg.conn, player);
			netMsg.conn.RemovePlayerController(s_RemovePlayerMessage.playerControllerId);
			if (m_MigrationManager != null)
			{
				m_MigrationManager.SendPeerInfo();
			}
		}

		internal void OnServerErrorInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerErrorInternal");
			}
			netMsg.ReadMessage<ErrorMessage>(s_ErrorMessage);
			this.OnServerError(netMsg.conn, s_ErrorMessage.errorCode);
		}

		internal void OnClientConnectInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientConnectInternal");
			}
			netMsg.conn.SetMaxDelay(m_MaxDelay);
			string name = SceneManager.GetSceneAt(0).name;
			if (string.IsNullOrEmpty(m_OnlineScene) || m_OnlineScene == m_OfflineScene || name == m_OnlineScene)
			{
				m_ClientLoadedScene = false;
				OnClientConnect(netMsg.conn);
			}
			else
			{
				s_ClientReadyConnection = netMsg.conn;
			}
		}

		internal void OnClientDisconnectInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientDisconnectInternal");
			}
			if (m_MigrationManager != null)
			{
				if (m_MigrationManager.LostHostOnClient(netMsg.conn))
				{
					return;
				}
			}
			if (!string.IsNullOrEmpty(m_OfflineScene))
			{
				ClientChangeScene(m_OfflineScene, false);
			}
			if (matchMaker != null && matchInfo != null && matchInfo.networkId != NetworkID.Invalid && matchInfo.nodeId != NodeID.Invalid)
			{
				matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, matchInfo.domain, new NetworkMatch.BasicResponseDelegate(OnDropConnection));
			}
			OnClientDisconnect(netMsg.conn);
		}

		internal void OnClientNotReadyMessageInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientNotReadyMessageInternal");
			}
			QSBClientScene.SetNotReady();
			OnClientNotReady(netMsg.conn);
		}

		internal void OnClientErrorInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientErrorInternal");
			}
			netMsg.ReadMessage<ErrorMessage>(s_ErrorMessage);
			this.OnClientError(netMsg.conn, s_ErrorMessage.errorCode);
		}

		internal void OnClientSceneInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientSceneInternal");
			}
			string newSceneName = netMsg.reader.ReadString();
			if (IsClientConnected() && !QSBNetworkServer.active)
			{
				ClientChangeScene(newSceneName, true);
			}
		}

		public virtual void OnServerConnect(QSBNetworkConnection conn)
		{
		}

		public virtual void OnServerDisconnect(QSBNetworkConnection conn)
		{
			QSBNetworkServer.DestroyPlayersForConnection(conn);
			if (conn.LastError != NetworkError.Ok)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ServerDisconnected due to error: " + conn.LastError);
				}
			}
		}

		public virtual void OnServerReady(QSBNetworkConnection conn)
		{
			if (conn.PlayerControllers.Count == 0)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("Ready with no player object");
				}
			}
			QSBNetworkServer.SetClientReady(conn);
		}

		public virtual void OnServerAddPlayer(QSBNetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
		{
			OnServerAddPlayerInternal(conn, playerControllerId);
		}

		public virtual void OnServerAddPlayer(QSBNetworkConnection conn, short playerControllerId)
		{
			OnServerAddPlayerInternal(conn, playerControllerId);
		}

		private void OnServerAddPlayerInternal(QSBNetworkConnection conn, short playerControllerId)
		{
			if (m_PlayerPrefab == null)
			{
				if (LogFilter.logError)
				{
					DebugLog.ToConsole("Error - The PlayerPrefab is empty on the QSBNetworkManager. Please setup a PlayerPrefab object.", MessageType.Error);
				}
			}
			else if (m_PlayerPrefab.GetComponent<QSBNetworkIdentity>() == null)
			{
				if (LogFilter.logError)
				{
					DebugLog.ToConsole("Error - The PlayerPrefab does not have a QSBNetworkIdentity. Please add a QSBNetworkIdentity to the player prefab.", MessageType.Error);
				}
			}
			else if ((int)playerControllerId < conn.PlayerControllers.Count && conn.PlayerControllers[(int)playerControllerId].IsValid && conn.PlayerControllers[(int)playerControllerId].gameObject != null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("There is already a player at that playerControllerId for this connections.");
				}
			}
			else
			{
				Transform startPosition = GetStartPosition();
				GameObject player;
				if (startPosition != null)
				{
					player = Instantiate<GameObject>(m_PlayerPrefab, startPosition.position, startPosition.rotation);
				}
				else
				{
					player = Instantiate<GameObject>(m_PlayerPrefab, Vector3.zero, Quaternion.identity);
				}
				QSBNetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
			}
		}

		public Transform GetStartPosition()
		{
			if (s_StartPositions.Count > 0)
			{
				for (int i = s_StartPositions.Count - 1; i >= 0; i--)
				{
					if (s_StartPositions[i] == null)
					{
						s_StartPositions.RemoveAt(i);
					}
				}
			}
			Transform result;
			if (m_PlayerSpawnMethod == PlayerSpawnMethod.Random && s_StartPositions.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, s_StartPositions.Count);
				result = s_StartPositions[index];
			}
			else if (m_PlayerSpawnMethod == PlayerSpawnMethod.RoundRobin && s_StartPositions.Count > 0)
			{
				if (s_StartPositionIndex >= s_StartPositions.Count)
				{
					s_StartPositionIndex = 0;
				}
				Transform transform = s_StartPositions[s_StartPositionIndex];
				s_StartPositionIndex++;
				result = transform;
			}
			else
			{
				result = null;
			}
			return result;
		}

		public virtual void OnServerRemovePlayer(QSBNetworkConnection conn, QSBPlayerController player)
		{
			if (player.gameObject != null)
			{
				QSBNetworkServer.Destroy(player.gameObject);
			}
		}

		public virtual void OnServerError(QSBNetworkConnection conn, int errorCode)
		{
		}

		public virtual void OnServerSceneChanged(string sceneName)
		{
		}

		public virtual void OnClientConnect(QSBNetworkConnection conn)
		{
			if (!clientLoadedScene)
			{
				QSBClientScene.Ready(conn);
				if (m_AutoCreatePlayer)
				{
					QSBClientScene.AddPlayer(0);
				}
			}
		}

		public virtual void OnClientDisconnect(QSBNetworkConnection conn)
		{
			StopClient();
			if (conn.LastError != NetworkError.Ok)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ClientDisconnected due to error: " + conn.LastError);
				}
			}
		}

		public virtual void OnClientError(QSBNetworkConnection conn, int errorCode)
		{
		}

		public virtual void OnClientNotReady(QSBNetworkConnection conn)
		{
		}

		public virtual void OnClientSceneChanged(QSBNetworkConnection conn)
		{
			QSBClientScene.Ready(conn);
			if (m_AutoCreatePlayer)
			{
				bool flag = QSBClientScene.localPlayers.Count == 0;
				bool flag2 = false;
				for (int i = 0; i < QSBClientScene.localPlayers.Count; i++)
				{
					if (QSBClientScene.localPlayers[i].gameObject != null)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					flag = true;
				}
				if (flag)
				{
					QSBClientScene.AddPlayer(0);
				}
			}
		}

		public void StartMatchMaker()
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager StartMatchMaker");
			}
			SetMatchHost(m_MatchHost, m_MatchPort, m_MatchPort == 443);
		}

		public void StopMatchMaker()
		{
			if (matchMaker != null && matchInfo != null && matchInfo.networkId != NetworkID.Invalid && matchInfo.nodeId != NodeID.Invalid)
			{
				matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, matchInfo.domain, new NetworkMatch.BasicResponseDelegate(OnDropConnection));
			}
			if (matchMaker != null)
			{
				Destroy(matchMaker);
				matchMaker = null;
			}
			matchInfo = null;
			matches = null;
		}

		public void SetMatchHost(string newHost, int port, bool https)
		{
			if (matchMaker == null)
			{
				matchMaker = base.gameObject.AddComponent<NetworkMatch>();
			}
			if (newHost == "127.0.0.1")
			{
				newHost = "localhost";
			}
			string text = "http://";
			if (https)
			{
				text = "https://";
			}
			if (newHost.StartsWith("http://"))
			{
				newHost = newHost.Replace("http://", "");
			}
			if (newHost.StartsWith("https://"))
			{
				newHost = newHost.Replace("https://", "");
			}
			m_MatchHost = newHost;
			m_MatchPort = port;
			string text2 = string.Concat(new object[]
			{
				text,
				m_MatchHost,
				":",
				m_MatchPort
			});
			if (LogFilter.logDebug)
			{
				Debug.Log("SetMatchHost:" + text2);
			}
			matchMaker.baseUri = new Uri(text2);
		}

		public virtual void OnStartHost()
		{
		}

		public virtual void OnStartServer()
		{
		}

		public virtual void OnStartClient(QSBNetworkClient client)
		{
		}

		public virtual void OnStopServer()
		{
		}

		public virtual void OnStopClient()
		{
		}

		public virtual void OnStopHost()
		{
		}

		public virtual void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogFormat("NetworkManager OnMatchCreate Success:{0}, ExtendedInfo:{1}, matchInfo:{2}", new object[]
				{
					success,
					extendedInfo,
					matchInfo
				});
			}
			if (success)
			{
				StartHost(matchInfo);
			}
		}

		public virtual void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogFormat("NetworkManager OnMatchList Success:{0}, ExtendedInfo:{1}, matchList.Count:{2}", new object[]
				{
					success,
					extendedInfo,
					matchList.Count
				});
			}
			matches = matchList;
		}

		public virtual void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogFormat("NetworkManager OnMatchJoined Success:{0}, ExtendedInfo:{1}, matchInfo:{2}", new object[]
				{
					success,
					extendedInfo,
					matchInfo
				});
			}
			if (success)
			{
				StartClient(matchInfo);
			}
		}

		public virtual void OnDestroyMatch(bool success, string extendedInfo)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogFormat("NetworkManager OnDestroyMatch Success:{0}, ExtendedInfo:{1}", new object[]
				{
					success,
					extendedInfo
				});
			}
		}

		public virtual void OnDropConnection(bool success, string extendedInfo)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogFormat("NetworkManager OnDropConnection Success:{0}, ExtendedInfo:{1}", new object[]
				{
					success,
					extendedInfo
				});
			}
		}

		public virtual void OnSetMatchAttributes(bool success, string extendedInfo)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogFormat("NetworkManager OnSetMatchAttributes Success:{0}, ExtendedInfo:{1}", new object[]
				{
					success,
					extendedInfo
				});
			}
		}

		[SerializeField]
		private int m_NetworkPort = 7777;

		[SerializeField]
		private bool m_ServerBindToIP;

		[SerializeField]
		private string m_ServerBindAddress = "";

		[SerializeField]
		private string m_NetworkAddress = "localhost";

		[SerializeField]
		private bool m_DontDestroyOnLoad = true;

		[SerializeField]
		private bool m_RunInBackground = true;

		[SerializeField]
		private bool m_ScriptCRCCheck = true;

		[SerializeField]
		private float m_MaxDelay = 0.01f;

		[SerializeField]
		private LogFilter.FilterLevel m_LogLevel = LogFilter.FilterLevel.Info;

		[SerializeField]
		private GameObject m_PlayerPrefab;

		[SerializeField]
		private bool m_AutoCreatePlayer = true;

		[SerializeField]
		private PlayerSpawnMethod m_PlayerSpawnMethod;

		[SerializeField]
		private string m_OfflineScene = "";

		[SerializeField]
		private string m_OnlineScene = "";

		[SerializeField]
		private List<GameObject> m_SpawnPrefabs = new List<GameObject>();

		[SerializeField]
		private bool m_CustomConfig;

		[SerializeField]
		private int m_MaxConnections = 4;

		[SerializeField]
		private ConnectionConfig m_ConnectionConfig;

		[SerializeField]
		private GlobalConfig m_GlobalConfig;

		[SerializeField]
		private List<QosType> m_Channels = new List<QosType>();

		[SerializeField]
		private bool m_UseWebSockets;

		[SerializeField]
		private bool m_UseSimulator;

		[SerializeField]
		private int m_SimulatedLatency = 1;

		[SerializeField]
		private float m_PacketLossPercentage;

		[SerializeField]
		private int m_MaxBufferedPackets = 16;

		[SerializeField]
		private bool m_AllowFragmentation = true;

		[SerializeField]
		private string m_MatchHost = "mm.unet.unity3d.com";

		[SerializeField]
		private int m_MatchPort = 443;

		[SerializeField]
		public string matchName = "default";

		[SerializeField]
		public uint matchSize = 4U;

		private QSBNetworkMigrationManager m_MigrationManager;

		private EndPoint m_EndPoint;

		private bool m_ClientLoadedScene;

		public static string networkSceneName = "";

		public bool isNetworkActive;

		public QSBNetworkClient client;

		private static List<Transform> s_StartPositions = new List<Transform>();

		private static int s_StartPositionIndex;

		public MatchInfo matchInfo;

		public NetworkMatch matchMaker;

		public List<MatchInfoSnapshot> matches;

		public static QSBNetworkManagerUNET singleton;

		private static AddPlayerMessage s_AddPlayerMessage = new AddPlayerMessage();

		private static RemovePlayerMessage s_RemovePlayerMessage = new RemovePlayerMessage();

		private static ErrorMessage s_ErrorMessage = new ErrorMessage();

		private static AsyncOperation s_LoadingSceneAsync;

		private static QSBNetworkConnection s_ClientReadyConnection;

		private static string s_Address;
	}
}