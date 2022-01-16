﻿using QuantumUNET.Logging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET.Components
{
	public class QNetworkManager : MonoBehaviour
	{
		public static QNetworkManager singleton;

		public int networkPort { get; set; } = 7777;
		public bool serverBindToIP { get; set; }
		public bool dontDestroyOnLoad { get; set; } = true;
		public bool runInBackground { get; set; } = true;
		public bool scriptCRCCheck { get; set; } = true;
		public bool autoCreatePlayer { get; set; } = true;
		public bool isNetworkActive;
		public bool clientLoadedScene { get; set; }
		public string serverBindAddress { get; set; } = "";
		public string networkAddress { get; set; } = "localhost";
		public float packetLossPercentage { get; set; }
		public float maxDelay { get; set; } = 0.01f;
		public GameObject playerPrefab { get; set; }
		public List<GameObject> spawnPrefabs { get; } = new List<GameObject>();
		public QNetworkClient client;
		public int maxConnections { get; set; } = 4;
		public List<QosType> channels { get; } = new List<QosType>();

		private ConnectionConfig m_ConnectionConfig;
		private GlobalConfig m_GlobalConfig;
		public int m_MaxBufferedPackets = 16;
		private readonly bool m_AllowFragmentation = true;
		private static readonly QAddPlayerMessage s_AddPlayerMessage = new();
		private static readonly QRemovePlayerMessage s_RemovePlayerMessage = new();
		private static readonly QErrorMessage s_ErrorMessage = new();
		private static string s_Address;

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
				foreach (var networkConnection in QNetworkServer.connections)
				{
					if (networkConnection != null)
					{
						foreach (var controller in networkConnection.PlayerControllers)
						{
							if (controller.IsValid)
							{
								num++;
							}
						}
					}
				}

				return num;
			}
		}

		public void Awake() => InitializeSingleton();

		private void InitializeSingleton()
		{
			if (!(singleton != null) || !(singleton == this))
			{
				if (dontDestroyOnLoad)
				{
					if (singleton != null)
					{
						QLog.Warning("Multiple NetworkManagers detected in the scene. Only one NetworkManager can exist at a time. The duplicate NetworkManager will not be used.");
						Destroy(gameObject);
						return;
					}

					QLog.Log("NetworkManager created singleton (DontDestroyOnLoad)");
					singleton = this;
					if (Application.isPlaying)
					{
						DontDestroyOnLoad(gameObject);
					}
				}
				else
				{
					QLog.Log("NetworkManager created singleton (ForScene)");
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
			QNetworkServer.RegisterHandler(QMsgType.Connect, OnServerConnectInternal);
			QNetworkServer.RegisterHandler(QMsgType.Disconnect, OnServerDisconnectInternal);
			QNetworkServer.RegisterHandler(QMsgType.Ready, OnServerReadyMessageInternal);
			QNetworkServer.RegisterHandler(QMsgType.AddPlayer, OnServerAddPlayerMessageInternal);
			QNetworkServer.RegisterHandler(QMsgType.RemovePlayer, OnServerRemovePlayerMessageInternal);
			QNetworkServer.RegisterHandler(QMsgType.Error, OnServerErrorInternal);
		}

		public bool StartServer() => StartServer(null, -1);

		private bool StartServer(ConnectionConfig config, int maxConnections)
		{
			InitializeSingleton();
			OnStartServer();
			if (runInBackground)
			{
				Application.runInBackground = true;
			}

			QNetworkCRC.scriptCRCCheck = scriptCRCCheck;
			if (m_GlobalConfig != null)
			{
				NetworkTransport.Init(m_GlobalConfig);
			}

			if (customConfig && m_ConnectionConfig != null && config == null)
			{
				m_ConnectionConfig.Channels.Clear();
				foreach (var channel in channels)
				{
					m_ConnectionConfig.AddChannel(channel);
				}

				QNetworkServer.Configure(m_ConnectionConfig, this.maxConnections);
			}

			if (config != null)
			{
				QNetworkServer.Configure(config, maxConnections);
			}

			if (serverBindToIP && !string.IsNullOrEmpty(serverBindAddress))
			{
				if (!QNetworkServer.Listen(serverBindAddress, networkPort))
				{
					QLog.FatalError($"StartServer listen on {serverBindAddress} failed.");
					return false;
				}
			}
			else if (!QNetworkServer.Listen(networkPort))
			{
				QLog.FatalError("StartServer listen failed.");
				return false;
			}

			RegisterServerMessages();
			QLog.Log($"NetworkManager StartServer port:{networkPort}");
			isNetworkActive = true;
			QNetworkServer.SpawnObjects();

			return true;
		}

		internal void RegisterClientMessages(QNetworkClient client)
		{
			client.RegisterHandler(QMsgType.Connect, OnClientConnectInternal);
			client.RegisterHandler(QMsgType.Disconnect, OnClientDisconnectInternal);
			client.RegisterHandler(QMsgType.NotReady, OnClientNotReadyMessageInternal);
			client.RegisterHandler(QMsgType.Error, OnClientErrorInternal);
			if (playerPrefab != null)
			{
				QClientScene.RegisterPrefab(playerPrefab);
			}

			foreach (var gameObject in spawnPrefabs)
			{
				if (gameObject != null)
				{
					QClientScene.RegisterPrefab(gameObject);
				}
			}
		}

		public QNetworkClient StartClient(ConnectionConfig config, int hostPort)
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

			client = new QNetworkClient
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
				foreach (var channel in channels)
				{
					m_ConnectionConfig.AddChannel(channel);
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
				QLog.Error("Must set the Network Address field in the manager");
				return null;
			}

			client.Connect(networkAddress, networkPort);
			OnStartClient(client);
			s_Address = networkAddress;
			return client;
		}

		public QNetworkClient StartClient() => StartClient(null);

		public QNetworkClient StartClient(ConnectionConfig config) => StartClient(config, 0);

		public virtual QNetworkClient StartHost(ConnectionConfig config, int maxConnections)
		{
			OnStartHost();
			QNetworkClient result;
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

		public virtual QNetworkClient StartHost()
		{
			OnStartHost();
			QNetworkClient result;
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

		private QNetworkClient ConnectLocalClient()
		{
			QLog.Log($"NetworkManager StartHost port:{networkPort}");
			networkAddress = "localhost";
			client = QClientScene.ConnectLocalServer();
			RegisterClientMessages(client);
			return client;
		}

		public void StopHost()
		{
			OnStopHost();
			StopClient();
			StopServer();
		}

		public void StopServer()
		{
			if (QNetworkServer.active)
			{
				OnStopServer();
				QLog.Log("NetworkManager StopServer");
				isNetworkActive = false;
				QNetworkServer.Shutdown();

				CleanupNetworkIdentities();
			}
		}

		public void StopClient()
		{
			OnStopClient();
			QLog.Log("NetworkManager StopClient");
			isNetworkActive = false;
			if (client != null)
			{
				client.Disconnect();
				client.Shutdown();
				client = null;
			}

			QClientScene.DestroyAllClientObjects();

			CleanupNetworkIdentities();
		}

		private void CleanupNetworkIdentities()
		{
			foreach (var networkIdentity in Resources.FindObjectsOfTypeAll<QNetworkIdentity>())
			{
				networkIdentity.MarkForReset();
			}
		}

		public bool IsClientConnected() => client != null && client.isConnected;

		public static void Shutdown()
		{
			if (!(singleton == null))
			{
				singleton.StopHost();
				singleton = null;
			}
		}

		internal void OnServerConnectInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnServerConnectInternal");
			netMsg.Connection.SetMaxDelay(maxDelay);
			if (m_MaxBufferedPackets != 512)
			{
				for (var i = 0; i < QNetworkServer.numChannels; i++)
				{
					netMsg.Connection.SetChannelOption(i, QChannelOption.MaxPendingBuffers, m_MaxBufferedPackets);
				}
			}

			if (!m_AllowFragmentation)
			{
				for (var j = 0; j < QNetworkServer.numChannels; j++)
				{
					netMsg.Connection.SetChannelOption(j, QChannelOption.AllowFragmentation, 0);
				}
			}

			OnServerConnect(netMsg.Connection);
		}

		internal void OnServerDisconnectInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnServerDisconnectInternal");
			OnServerDisconnect(netMsg.Connection);
		}

		internal void OnServerReadyMessageInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnServerReadyMessageInternal");
			OnServerReady(netMsg.Connection);
		}

		internal void OnServerAddPlayerMessageInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnServerAddPlayerMessageInternal");
			netMsg.ReadMessage(s_AddPlayerMessage);
			if (s_AddPlayerMessage.msgSize != 0)
			{
				var extraMessageReader = new QNetworkReader(s_AddPlayerMessage.msgData);
				OnServerAddPlayer(netMsg.Connection, s_AddPlayerMessage.playerControllerId, extraMessageReader);
			}
			else
			{
				OnServerAddPlayer(netMsg.Connection, s_AddPlayerMessage.playerControllerId);
			}
		}

		internal void OnServerRemovePlayerMessageInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnServerRemovePlayerMessageInternal");
			netMsg.ReadMessage(s_RemovePlayerMessage);
			netMsg.Connection.GetPlayerController(s_RemovePlayerMessage.PlayerControllerId, out var player);
			OnServerRemovePlayer(netMsg.Connection, player);
			netMsg.Connection.RemovePlayerController(s_RemovePlayerMessage.PlayerControllerId);
		}

		internal void OnServerErrorInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnServerErrorInternal");
			netMsg.ReadMessage(s_ErrorMessage);
			OnServerError(netMsg.Connection, s_ErrorMessage.errorCode);
		}

		internal void OnClientConnectInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnClientConnectInternal");
			netMsg.Connection.SetMaxDelay(maxDelay);
			clientLoadedScene = false;
			OnClientConnect(netMsg.Connection);
		}

		internal void OnClientDisconnectInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnClientDisconnectInternal");

			OnClientDisconnect(netMsg.Connection);
		}

		internal void OnClientNotReadyMessageInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnClientNotReadyMessageInternal");
			QClientScene.SetNotReady();
			OnClientNotReady(netMsg.Connection);
		}

		internal void OnClientErrorInternal(QNetworkMessage netMsg)
		{
			QLog.Log("NetworkManager:OnClientErrorInternal");
			netMsg.ReadMessage(s_ErrorMessage);
			OnClientError(netMsg.Connection, s_ErrorMessage.errorCode);
		}

		public virtual void OnServerConnect(QNetworkConnection conn)
		{
		}

		public virtual void OnServerDisconnect(QNetworkConnection conn)
		{
			QNetworkServer.DestroyPlayersForConnection(conn);
			if (conn.LastError != NetworkError.Ok)
			{
				QLog.Error($"ServerDisconnected due to error: {conn.LastError}");
			}
		}

		public virtual void OnServerReady(QNetworkConnection conn)
		{
			if (conn.PlayerControllers.Count == 0)
			{
				QLog.Warning("Ready with no player object");
			}

			QNetworkServer.SetClientReady(conn);
		}

		public virtual void OnServerAddPlayer(QNetworkConnection conn, short playerControllerId, QNetworkReader extraMessageReader) => OnServerAddPlayerInternal(conn, playerControllerId);

		public virtual void OnServerAddPlayer(QNetworkConnection conn, short playerControllerId) => OnServerAddPlayerInternal(conn, playerControllerId);

		private void OnServerAddPlayerInternal(QNetworkConnection conn, short playerControllerId)
		{
			if (playerPrefab == null)
			{
				QLog.FatalError("The PlayerPrefab is empty on the QSBNetworkManager. Please setup a PlayerPrefab object.");
			}
			else if (playerPrefab.GetComponent<QNetworkIdentity>() == null)
			{
				QLog.FatalError("The PlayerPrefab does not have a QSBNetworkIdentity. Please add a QSBNetworkIdentity to the player prefab.");
			}
			else if (playerControllerId < conn.PlayerControllers.Count && conn.PlayerControllers[playerControllerId].IsValid && conn.PlayerControllers[playerControllerId].Gameobject != null)
			{
				QLog.Warning("There is already a player at that playerControllerId for this connections.");
			}
			else
			{
				var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
				QNetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
			}
		}

		public virtual void OnServerRemovePlayer(QNetworkConnection conn, QPlayerController player)
		{
			if (player.Gameobject != null)
			{
				QNetworkServer.Destroy(player.Gameobject);
			}
		}

		public virtual void OnServerError(QNetworkConnection conn, int errorCode)
		{
		}

		public virtual void OnServerSceneChanged(string sceneName)
		{
		}

		public virtual void OnClientConnect(QNetworkConnection conn)
		{
			if (!clientLoadedScene)
			{
				QClientScene.Ready(conn);
				if (autoCreatePlayer)
				{
					QClientScene.AddPlayer(0);
				}
			}
		}

		public virtual void OnClientDisconnect(QNetworkConnection conn)
		{
			StopClient();
			if (conn.LastError != NetworkError.Ok)
			{
				QLog.Error($"ClientDisconnected due to error: {conn.LastError}");
			}
		}

		public virtual void OnClientError(QNetworkConnection conn, int errorCode)
		{
		}

		public virtual void OnClientNotReady(QNetworkConnection conn)
		{
		}

		public virtual void OnClientSceneChanged(QNetworkConnection conn)
		{
			QClientScene.Ready(conn);
			if (autoCreatePlayer)
			{
				var flag = QClientScene.localPlayers.Count == 0;
				var flag2 = false;
				foreach (var player in QClientScene.localPlayers)
				{
					if (player.Gameobject != null)
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
					QClientScene.AddPlayer(0);
				}
			}
		}

		public virtual void OnStartHost()
		{
		}

		public virtual void OnStartServer()
		{
		}

		public virtual void OnStartClient(QNetworkClient client)
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