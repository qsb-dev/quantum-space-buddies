﻿using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET
{
	public class QNetworkServerSimple
	{
		public QNetworkServerSimple() => connections = new ReadOnlyCollection<QNetworkConnection>(m_Connections);
		public int listenPort { get; set; }
		public int serverHostId { get; set; } = -1;
		public HostTopology hostTopology { get; private set; }
		public ReadOnlyCollection<QNetworkConnection> connections { get; }
		public Dictionary<short, QNetworkMessageDelegate> handlers => m_MessageHandlers.GetHandlers();
		public byte[] messageBuffer { get; private set; }
		public QNetworkReader messageReader { get; private set; }
		public Type networkConnectionClass { get; private set; } = typeof(QNetworkConnection);

		public void SetNetworkConnectionClass<T>()
			where T : QNetworkConnection
			=> networkConnectionClass = typeof(T);

		public virtual void Initialize()
		{
			if (!m_Initialized)
			{
				m_Initialized = true;
				NetworkTransport.Init();
				messageBuffer = new byte[65535];
				messageReader = new QNetworkReader(messageBuffer);
				if (hostTopology == null)
				{
					var connectionConfig = new ConnectionConfig();
					connectionConfig.AddChannel(QosType.ReliableSequenced);
					connectionConfig.AddChannel(QosType.Unreliable);
					hostTopology = new HostTopology(connectionConfig, 8);
				}

				Debug.Log("NetworkServerSimple initialize.");
			}
		}

		public bool Configure(ConnectionConfig config, int maxConnections)
		{
			var topology = new HostTopology(config, maxConnections);
			return Configure(topology);
		}

		public bool Configure(HostTopology topology)
		{
			hostTopology = topology;
			return true;
		}

		public bool Listen(string ipAddress, int serverListenPort)
		{
			Initialize();
			listenPort = serverListenPort;
			serverHostId = NetworkTransport.AddHost(hostTopology, serverListenPort, ipAddress);
			bool result;
			if (serverHostId == -1)
			{
				result = false;
			}
			else
			{
				Debug.Log($"NetworkServerSimple listen: {ipAddress}:{listenPort}");
				result = true;
			}

			return result;
		}

		public bool Listen(int serverListenPort)
			=> Listen(serverListenPort, hostTopology);

		public bool Listen(int serverListenPort, HostTopology topology)
		{
			hostTopology = topology;
			Initialize();
			listenPort = serverListenPort;
			serverHostId = NetworkTransport.AddHost(hostTopology, serverListenPort);
			bool result;
			if (serverHostId == -1)
			{
				result = false;
			}
			else
			{
				Debug.Log($"NetworkServerSimple listen {listenPort}");
				result = true;
			}

			return result;
		}

		public void Stop()
		{
			Debug.Log("NetworkServerSimple stop ");
			NetworkTransport.RemoveHost(serverHostId);
			serverHostId = -1;
		}

		public void RegisterHandlerSafe(short msgType, QNetworkMessageDelegate handler)
			=> m_MessageHandlers.RegisterHandlerSafe(msgType, handler);

		public void RegisterHandler(short msgType, QNetworkMessageDelegate handler)
			=> m_MessageHandlers.RegisterHandler(msgType, handler);

		public void UnregisterHandler(short msgType)
			=> m_MessageHandlers.UnregisterHandler(msgType);

		public void ClearHandlers()
			=> m_MessageHandlers.ClearMessageHandlers();

		public void UpdateConnections()
		{
			foreach (var networkConnection in m_Connections)
			{
				networkConnection?.FlushChannels();
			}
		}

		public void Update()
		{
			if (serverHostId != -1)
			{
				NetworkEventType networkEventType;
				if (m_RelaySlotId != -1)
				{
					networkEventType = NetworkTransport.ReceiveRelayEventFromHost(serverHostId, out var b);
					if (networkEventType != NetworkEventType.Nothing)
					{
						Debug.Log($"NetGroup event:{networkEventType}");
					}

					if (networkEventType == NetworkEventType.ConnectEvent)
					{
						Debug.Log("NetGroup server connected");
					}

					if (networkEventType == NetworkEventType.DisconnectEvent)
					{
						Debug.Log("NetGroup server disconnected");
					}
				}

				do
				{
					networkEventType = NetworkTransport.ReceiveFromHost(serverHostId, out var connectionId, out var channelId, messageBuffer, messageBuffer.Length, out var receivedSize, out var b);

					switch (networkEventType)
					{
						case NetworkEventType.DataEvent:
							HandleData(connectionId, channelId, receivedSize, b);
							break;

						case NetworkEventType.ConnectEvent:
							HandleConnect(connectionId, b);
							break;

						case NetworkEventType.DisconnectEvent:
							HandleDisconnect(connectionId, b);
							break;

						case NetworkEventType.Nothing:
							break;

						default:
							Debug.LogError($"Unknown network message type received: {networkEventType}");
							break;
					}
				}
				while (networkEventType != NetworkEventType.Nothing);
				UpdateConnections();
			}
		}

		public QNetworkConnection FindConnection(int connectionId)
		{
			QNetworkConnection result;
			if (connectionId < 0 || connectionId >= m_Connections.Count)
			{
				result = null;
			}
			else
			{
				result = m_Connections[connectionId];
			}

			return result;
		}

		public bool SetConnectionAtIndex(QNetworkConnection conn)
		{
			while (m_Connections.Count <= conn.connectionId)
			{
				m_Connections.Add(null);
			}

			bool result;
			if (m_Connections[conn.connectionId] != null)
			{
				result = false;
			}
			else
			{
				m_Connections[conn.connectionId] = conn;
				conn.SetHandlers(m_MessageHandlers);
				result = true;
			}

			return result;
		}

		public bool RemoveConnectionAtIndex(int connectionId)
		{
			bool result;
			if (connectionId < 0 || connectionId >= m_Connections.Count)
			{
				result = false;
			}
			else
			{
				m_Connections[connectionId] = null;
				result = true;
			}

			return result;
		}

		private void HandleConnect(int connectionId, byte error)
		{
			Debug.Log($"NetworkServerSimple accepted client:{connectionId}");
			if (error != 0)
			{
				OnConnectError(connectionId, error);
			}
			else
			{
				NetworkTransport.GetConnectionInfo(serverHostId, connectionId, out var networkAddress, out var num, out var networkID, out var nodeID, out var lastError);
				var networkConnection = (QNetworkConnection)Activator.CreateInstance(networkConnectionClass);
				networkConnection.SetHandlers(m_MessageHandlers);
				networkConnection.Initialize(networkAddress, serverHostId, connectionId, hostTopology);
				networkConnection.LastError = (NetworkError)lastError;
				while (m_Connections.Count <= connectionId)
				{
					m_Connections.Add(null);
				}

				m_Connections[connectionId] = networkConnection;
				OnConnected(networkConnection);
			}
		}

		private void HandleDisconnect(int connectionId, byte error)
		{
			Debug.Log($"NetworkServerSimple disconnect client:{connectionId}");
			var networkConnection = FindConnection(connectionId);
			if (networkConnection != null)
			{
				networkConnection.LastError = (NetworkError)error;
				if (error != 0)
				{
					if (error != 6)
					{
						m_Connections[connectionId] = null;
						Debug.LogError(
							$"Server client disconnect error, connectionId: {connectionId} error: {(NetworkError)error}");
						OnDisconnectError(networkConnection, error);
						return;
					}
				}

				networkConnection.Disconnect();
				m_Connections[connectionId] = null;
				Debug.Log($"Server lost client:{connectionId}");
				OnDisconnected(networkConnection);
			}
		}

		private void HandleData(int connectionId, int channelId, int receivedSize, byte error)
		{
			var networkConnection = FindConnection(connectionId);
			if (networkConnection == null)
			{
				Debug.LogError($"HandleData Unknown connectionId:{connectionId}");
			}
			else
			{
				networkConnection.LastError = (NetworkError)error;
				if (error != 0)
				{
					OnDataError(networkConnection, error);
				}
				else
				{
					messageReader.SeekZero();
					OnData(networkConnection, receivedSize, channelId);
				}
			}
		}

		public void SendBytesTo(int connectionId, byte[] bytes, int numBytes, int channelId)
		{
			var networkConnection = FindConnection(connectionId);
			networkConnection?.SendBytes(bytes, numBytes, channelId);
		}

		public void SendWriterTo(int connectionId, QNetworkWriter writer, int channelId)
		{
			var networkConnection = FindConnection(connectionId);
			networkConnection?.SendWriter(writer, channelId);
		}

		public void Disconnect(int connectionId)
		{
			var networkConnection = FindConnection(connectionId);
			if (networkConnection != null)
			{
				networkConnection.Disconnect();
				m_Connections[connectionId] = null;
			}
		}

		public void DisconnectAllConnections()
		{
			foreach (var networkConnection in m_Connections.Where(networkConnection => networkConnection != null))
			{
				networkConnection.Disconnect();
				networkConnection.Dispose();
			}
		}

		public virtual void OnConnectError(int connectionId, byte error)
			=> Debug.LogError($"OnConnectError error:{error}");

		public virtual void OnDataError(QNetworkConnection conn, byte error)
			=> Debug.LogError($"OnDataError error:{error}");

		public virtual void OnDisconnectError(QNetworkConnection conn, byte error)
			=> Debug.LogError($"OnDisconnectError error:{error}");

		public virtual void OnConnected(QNetworkConnection conn)
			=> conn.InvokeHandlerNoData(32);

		public virtual void OnDisconnected(QNetworkConnection conn)
			=> conn.InvokeHandlerNoData(33);

		public virtual void OnData(QNetworkConnection conn, int receivedSize, int channelId)
			=> conn.TransportReceive(messageBuffer, receivedSize, channelId);

		private bool m_Initialized;
		private int m_RelaySlotId = -1;
		private readonly List<QNetworkConnection> m_Connections = new();
		private readonly QNetworkMessageHandlers m_MessageHandlers = new();
	}
}