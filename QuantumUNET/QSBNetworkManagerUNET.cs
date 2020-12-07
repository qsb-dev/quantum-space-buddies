using OWML.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace QuantumUNET
{
	public class QSBNetworkManagerUNET : MonoBehaviour
	{
		public static QSBNetworkManagerUNET singleton;
		public static string networkSceneName = "";

		public int networkPort { get; set; } = 7777;
		public int simulatedLatency { get; set; } = 1;
		public bool serverBindToIP { get; set; }
		public bool dontDestroyOnLoad { get; set; } = true;
		public bool runInBackground { get; set; } = true;
		public bool scriptCRCCheck { get; set; } = true;
		public bool autoCreatePlayer { get; set; } = true;
		public bool isNetworkActive;
		public bool useWebSockets { get; set; }
		public bool useSimulator { get; set; }
		public bool clientLoadedScene { get; set; }
		public string serverBindAddress { get; set; } = "";
		public string networkAddress { get; set; } = "localhost";
		public string offlineScene { get; set; } = "";
		public string onlineScene { get; set; } = "";
		public float packetLossPercentage { get; set; }
		public float maxDelay { get; set; } = 0.01f;
		public GameObject playerPrefab { get; set; }
		public PlayerSpawnMethod playerSpawnMethod { get; set; }
		public List<GameObject> spawnPrefabs { get; } = new List<GameObject>();
		public QSBNetworkClient client;
		public int maxConnections { get; set; } = 4;
		public List<QosType> channels { get; } = new List<QosType>();

		private ConnectionConfig m_ConnectionConfig;
		private GlobalConfig m_GlobalConfig;
		private int m_MaxBufferedPackets = 16;
		private bool m_AllowFragmentation = true;
		private static List<Transform> s_StartPositions = new List<Transform>();
		private static int s_StartPositionIndex;
		private static QSBAddPlayerMessage s_AddPlayerMessage = new QSBAddPlayerMessage();
		private static QSBRemovePlayerMessage s_RemovePlayerMessage = new QSBRemovePlayerMessage();
		private static QSBErrorMessage s_ErrorMessage = new QSBErrorMessage();
		private static AsyncOperation s_LoadingSceneAsync;
		private static QSBNetworkConnection s_ClientReadyConnection;
		private static string s_Address;

		public List<Transform> startPositions
		{
			get
			{
				return s_StartPositions;
			}
		}

		public bool customConfig { get; set; }

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

		public int numPlayers
		{
			get
			{
				var num = 0;
				for (var i = 0; i < QSBNetworkServer.connections.Count; i++)
				{
					var networkConnection = QSBNetworkServer.connections[i];
					if (networkConnection != null)
					{
						for (var j = 0; j < networkConnection.PlayerControllers.Count; j++)
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
				if (dontDestroyOnLoad)
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
				if (networkAddress != "")
				{
					s_Address = networkAddress;
				}
				else if (s_Address != "")
				{
					networkAddress = s_Address;
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

		public bool StartServer()
		{
			return StartServer(null, -1);
		}

		private bool StartServer(ConnectionConfig config, int maxConnections)
		{
			InitializeSingleton();
			OnStartServer();
			if (runInBackground)
			{
				Application.runInBackground = true;
			}
			QSBNetworkCRC.scriptCRCCheck = scriptCRCCheck;
			QSBNetworkServer.useWebSockets = useWebSockets;
			if (m_GlobalConfig != null)
			{
				NetworkTransport.Init(m_GlobalConfig);
			}
			if (customConfig && m_ConnectionConfig != null && config == null)
			{
				m_ConnectionConfig.Channels.Clear();
				for (var i = 0; i < channels.Count; i++)
				{
					m_ConnectionConfig.AddChannel(channels[i]);
				}
				QSBNetworkServer.Configure(m_ConnectionConfig, this.maxConnections);
			}
			if (config != null)
			{
				QSBNetworkServer.Configure(config, maxConnections);
			}
			if (serverBindToIP && !string.IsNullOrEmpty(serverBindAddress))
			{
				if (!QSBNetworkServer.Listen(serverBindAddress, networkPort))
				{
					if (LogFilter.logError)
					{
						Debug.LogError("StartServer listen on " + serverBindAddress + " failed.");
					}
					return false;
				}
			}
			else if (!QSBNetworkServer.Listen(networkPort))
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
				Debug.Log("NetworkManager StartServer port:" + networkPort);
			}
			isNetworkActive = true;
			var name = SceneManager.GetSceneAt(0).name;
			if (!string.IsNullOrEmpty(onlineScene) && onlineScene != name && onlineScene != offlineScene)
			{
				ServerChangeScene(onlineScene);
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
			if (playerPrefab != null)
			{
				QSBClientScene.RegisterPrefab(playerPrefab);
			}
			for (var i = 0; i < spawnPrefabs.Count; i++)
			{
				var gameObject = spawnPrefabs[i];
				if (gameObject != null)
				{
					QSBClientScene.RegisterPrefab(gameObject);
				}
			}
		}

		public void UseExternalClient(QSBNetworkClient externalClient)
		{
			if (runInBackground)
			{
				Application.runInBackground = true;
			}
			if (externalClient != null)
			{
				client = externalClient;
				isNetworkActive = true;
				RegisterClientMessages(client);
				OnStartClient(client);
			}
			else
			{
				OnStopClient();
				QSBClientScene.DestroyAllClientObjects();
				QSBClientScene.HandleClientDisconnect(client.connection);
				client = null;
				if (!string.IsNullOrEmpty(offlineScene))
				{
					ClientChangeScene(offlineScene, false);
				}
			}
			s_Address = networkAddress;
		}

		public QSBNetworkClient StartClient(ConnectionConfig config, int hostPort)
		{
			InitializeSingleton();
			if (runInBackground)
			{
				Application.runInBackground = true;
			}
			isNetworkActive = true;
			if (m_GlobalConfig != null)
			{
				NetworkTransport.Init(m_GlobalConfig);
			}
			client = new QSBNetworkClient
			{
				hostPort = hostPort
			};
			if (config != null)
			{
				if (config.UsePlatformSpecificProtocols && Application.platform != RuntimePlatform.PS4 && Application.platform != RuntimePlatform.PSP2)
				{
					throw new ArgumentOutOfRangeException("Platform specific protocols are not supported on this platform");
				}
				client.Configure(config, 1);
			}
			else if (customConfig && m_ConnectionConfig != null)
			{
				m_ConnectionConfig.Channels.Clear();
				for (var i = 0; i < channels.Count; i++)
				{
					m_ConnectionConfig.AddChannel(channels[i]);
				}
				if (m_ConnectionConfig.UsePlatformSpecificProtocols && Application.platform != RuntimePlatform.PS4 && Application.platform != RuntimePlatform.PSP2)
				{
					throw new ArgumentOutOfRangeException("Platform specific protocols are not supported on this platform");
				}
				client.Configure(m_ConnectionConfig, maxConnections);
			}
			RegisterClientMessages(client);
			if (string.IsNullOrEmpty(networkAddress))
			{
				ModConsole.OwmlConsole.WriteLine("Must set the Network Address field in the manager");
				return null;
			}
			ModConsole.OwmlConsole.WriteLine(string.Concat(new object[]
			{
				"NetworkManager StartClient address:",
				networkAddress,
				" port:",
				networkPort
			}));
			if (useSimulator)
			{
				ModConsole.OwmlConsole.WriteLine("connecting with simulator");
				client.ConnectWithSimulator(networkAddress, networkPort, simulatedLatency, packetLossPercentage);
			}
			else
			{
				ModConsole.OwmlConsole.WriteLine("connecting");
				client.Connect(networkAddress, networkPort);
			}
			OnStartClient(client);
			s_Address = networkAddress;
			return client;
		}

		public QSBNetworkClient StartClient()
		{
			return StartClient(null);
		}

		public QSBNetworkClient StartClient(ConnectionConfig config)
		{
			return StartClient(config, 0);
		}

		public virtual QSBNetworkClient StartHost(ConnectionConfig config, int maxConnections)
		{
			OnStartHost();
			QSBNetworkClient result;
			if (StartServer(config, maxConnections))
			{
				var networkClient = ConnectLocalClient();
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

		public virtual QSBNetworkClient StartHost()
		{
			OnStartHost();
			QSBNetworkClient result;
			if (StartServer())
			{
				var networkClient = ConnectLocalClient();
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
				Debug.Log("NetworkManager StartHost port:" + networkPort);
			}
			networkAddress = "localhost";
			client = QSBClientScene.ConnectLocalServer();
			RegisterClientMessages(client);
			return client;
		}

		public void StopHost()
		{
			OnStopHost();
			StopServer();
			StopClient();
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
				if (!string.IsNullOrEmpty(offlineScene))
				{
					ServerChangeScene(offlineScene);
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
			QSBClientScene.DestroyAllClientObjects();
			if (!string.IsNullOrEmpty(offlineScene))
			{
				ClientChangeScene(offlineScene, false);
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
				var msg = new QSBStringMessage(networkSceneName);
				QSBNetworkServer.SendToAll(39, msg);
				s_StartPositionIndex = 0;
				s_StartPositions.Clear();
			}
		}

		private void CleanupNetworkIdentities()
		{
			foreach (var networkIdentity in Resources.FindObjectsOfTypeAll<QSBNetworkIdentity>())
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
					clientLoadedScene = true;
					OnClientConnect(s_ClientReadyConnection);
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
				RegisterClientMessages(client);
				OnClientSceneChanged(client.connection);
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
						ModConsole.OwmlConsole.WriteLine("ClientChangeScene done readyCon:" + s_ClientReadyConnection);
						singleton.FinishLoadScene();
						s_LoadingSceneAsync.allowSceneActivation = true;
						s_LoadingSceneAsync = null;
					}
				}
			}
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
			netMsg.Connection.SetMaxDelay(maxDelay);
			if (m_MaxBufferedPackets != 512)
			{
				for (var i = 0; i < QSBNetworkServer.numChannels; i++)
				{
					netMsg.Connection.SetChannelOption(i, ChannelOption.MaxPendingBuffers, m_MaxBufferedPackets);
				}
			}
			if (!m_AllowFragmentation)
			{
				for (var j = 0; j < QSBNetworkServer.numChannels; j++)
				{
					netMsg.Connection.SetChannelOption(j, ChannelOption.AllowFragmentation, 0);
				}
			}
			if (networkSceneName != "" && networkSceneName != offlineScene)
			{
				var msg = new QSBStringMessage(networkSceneName);
				netMsg.Connection.Send(39, msg);
			}
			OnServerConnect(netMsg.Connection);
		}

		internal void OnServerDisconnectInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerDisconnectInternal");
			}
			OnServerDisconnect(netMsg.Connection);
		}

		internal void OnServerReadyMessageInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerReadyMessageInternal");
			}
			OnServerReady(netMsg.Connection);
		}

		internal void OnServerAddPlayerMessageInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerAddPlayerMessageInternal");
			}
			netMsg.ReadMessage<QSBAddPlayerMessage>(s_AddPlayerMessage);
			if (s_AddPlayerMessage.msgSize != 0)
			{
				var extraMessageReader = new NetworkReader(s_AddPlayerMessage.msgData);
				OnServerAddPlayer(netMsg.Connection, s_AddPlayerMessage.playerControllerId, extraMessageReader);
			}
			else
			{
				OnServerAddPlayer(netMsg.Connection, s_AddPlayerMessage.playerControllerId);
			}
		}

		internal void OnServerRemovePlayerMessageInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerRemovePlayerMessageInternal");
			}
			netMsg.ReadMessage<QSBRemovePlayerMessage>(s_RemovePlayerMessage);
			netMsg.Connection.GetPlayerController(s_RemovePlayerMessage.PlayerControllerId, out var player);
			OnServerRemovePlayer(netMsg.Connection, player);
			netMsg.Connection.RemovePlayerController(s_RemovePlayerMessage.PlayerControllerId);
		}

		internal void OnServerErrorInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnServerErrorInternal");
			}
			netMsg.ReadMessage<QSBErrorMessage>(s_ErrorMessage);
			OnServerError(netMsg.Connection, s_ErrorMessage.errorCode);
		}

		internal void OnClientConnectInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientConnectInternal");
			}
			netMsg.Connection.SetMaxDelay(maxDelay);
			var name = SceneManager.GetSceneAt(0).name;
			if (string.IsNullOrEmpty(onlineScene) || onlineScene == offlineScene || name == onlineScene)
			{
				clientLoadedScene = false;
				OnClientConnect(netMsg.Connection);
			}
			else
			{
				s_ClientReadyConnection = netMsg.Connection;
			}
		}

		internal void OnClientDisconnectInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientDisconnectInternal");
			}
			if (!string.IsNullOrEmpty(offlineScene))
			{
				ClientChangeScene(offlineScene, false);
			}
			OnClientDisconnect(netMsg.Connection);
		}

		internal void OnClientNotReadyMessageInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientNotReadyMessageInternal");
			}
			QSBClientScene.SetNotReady();
			OnClientNotReady(netMsg.Connection);
		}

		internal void OnClientErrorInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientErrorInternal");
			}
			netMsg.ReadMessage<QSBErrorMessage>(s_ErrorMessage);
			OnClientError(netMsg.Connection, s_ErrorMessage.errorCode);
		}

		internal void OnClientSceneInternal(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkManager:OnClientSceneInternal");
			}
			var newSceneName = netMsg.Reader.ReadString();
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
			if (playerPrefab == null)
			{
				if (LogFilter.logError)
				{
					ModConsole.OwmlConsole.WriteLine("Error - The PlayerPrefab is empty on the QSBNetworkManager. Please setup a PlayerPrefab object.");
				}
			}
			else if (playerPrefab.GetComponent<QSBNetworkIdentity>() == null)
			{
				if (LogFilter.logError)
				{
					ModConsole.OwmlConsole.WriteLine("Error - The PlayerPrefab does not have a QSBNetworkIdentity. Please add a QSBNetworkIdentity to the player prefab.");
				}
			}
			else if (playerControllerId < conn.PlayerControllers.Count && conn.PlayerControllers[playerControllerId].IsValid && conn.PlayerControllers[playerControllerId].Gameobject != null)
			{
				if (LogFilter.logError)
				{
					ModConsole.OwmlConsole.WriteLine("Warning - There is already a player at that playerControllerId for this connections.");
				}
			}
			else
			{
				var startPosition = GetStartPosition();
				GameObject player;
				if (startPosition != null)
				{
					player = Instantiate<GameObject>(playerPrefab, startPosition.position, startPosition.rotation);
				}
				else
				{
					player = Instantiate<GameObject>(playerPrefab, Vector3.zero, Quaternion.identity);
				}
				QSBNetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
			}
		}

		public Transform GetStartPosition()
		{
			if (s_StartPositions.Count > 0)
			{
				for (var i = s_StartPositions.Count - 1; i >= 0; i--)
				{
					if (s_StartPositions[i] == null)
					{
						s_StartPositions.RemoveAt(i);
					}
				}
			}
			Transform result;
			if (playerSpawnMethod == PlayerSpawnMethod.Random && s_StartPositions.Count > 0)
			{
				var index = UnityEngine.Random.Range(0, s_StartPositions.Count);
				result = s_StartPositions[index];
			}
			else if (playerSpawnMethod == PlayerSpawnMethod.RoundRobin && s_StartPositions.Count > 0)
			{
				if (s_StartPositionIndex >= s_StartPositions.Count)
				{
					s_StartPositionIndex = 0;
				}
				var transform = s_StartPositions[s_StartPositionIndex];
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
			if (player.Gameobject != null)
			{
				QSBNetworkServer.Destroy(player.Gameobject);
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
				if (autoCreatePlayer)
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
			if (autoCreatePlayer)
			{
				var flag = QSBClientScene.localPlayers.Count == 0;
				var flag2 = false;
				for (var i = 0; i < QSBClientScene.localPlayers.Count; i++)
				{
					if (QSBClientScene.localPlayers[i].Gameobject != null)
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
	}
}