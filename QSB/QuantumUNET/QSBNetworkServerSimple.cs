using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

namespace QSB.QuantumUNET
{
	public class QSBNetworkServerSimple
	{
		public QSBNetworkServerSimple()
		{
			this.m_ConnectionsReadOnly = new ReadOnlyCollection<QSBNetworkConnection>(this.m_Connections);
		}

		public int listenPort
		{
			get
			{
				return this.m_ListenPort;
			}
			set
			{
				this.m_ListenPort = value;
			}
		}

		public int serverHostId
		{
			get
			{
				return this.m_ServerHostId;
			}
			set
			{
				this.m_ServerHostId = value;
			}
		}

		public HostTopology hostTopology
		{
			get
			{
				return this.m_HostTopology;
			}
		}

		public bool useWebSockets
		{
			get
			{
				return this.m_UseWebSockets;
			}
			set
			{
				this.m_UseWebSockets = value;
			}
		}

		public ReadOnlyCollection<QSBNetworkConnection> connections
		{
			get
			{
				return this.m_ConnectionsReadOnly;
			}
		}

		public Dictionary<short, QSBNetworkMessageDelegate> handlers
		{
			get
			{
				return this.m_MessageHandlers.GetHandlers();
			}
		}

		public byte[] messageBuffer
		{
			get
			{
				return this.m_MsgBuffer;
			}
		}

		public NetworkReader messageReader
		{
			get
			{
				return this.m_MsgReader;
			}
		}

		public Type networkConnectionClass
		{
			get
			{
				return this.m_NetworkConnectionClass;
			}
		}

		public void SetNetworkConnectionClass<T>() where T : QSBNetworkConnection
		{
			this.m_NetworkConnectionClass = typeof(T);
		}

		public virtual void Initialize()
		{
			if (!this.m_Initialized)
			{
				this.m_Initialized = true;
				NetworkTransport.Init();
				this.m_MsgBuffer = new byte[65535];
				this.m_MsgReader = new NetworkReader(this.m_MsgBuffer);
				if (this.m_HostTopology == null)
				{
					ConnectionConfig connectionConfig = new ConnectionConfig();
					connectionConfig.AddChannel(QosType.ReliableSequenced);
					connectionConfig.AddChannel(QosType.Unreliable);
					this.m_HostTopology = new HostTopology(connectionConfig, 8);
				}
				if (LogFilter.logDebug)
				{
					Debug.Log("NetworkServerSimple initialize.");
				}
			}
		}

		public bool Configure(ConnectionConfig config, int maxConnections)
		{
			HostTopology topology = new HostTopology(config, maxConnections);
			return this.Configure(topology);
		}

		public bool Configure(HostTopology topology)
		{
			this.m_HostTopology = topology;
			return true;
		}

		public bool Listen(string ipAddress, int serverListenPort)
		{
			this.Initialize();
			this.m_ListenPort = serverListenPort;
			if (this.m_UseWebSockets)
			{
				this.m_ServerHostId = NetworkTransport.AddWebsocketHost(this.m_HostTopology, serverListenPort, ipAddress);
			}
			else
			{
				this.m_ServerHostId = NetworkTransport.AddHost(this.m_HostTopology, serverListenPort, ipAddress);
			}
			bool result;
			if (this.m_ServerHostId == -1)
			{
				result = false;
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log(string.Concat(new object[]
					{
						"NetworkServerSimple listen: ",
						ipAddress,
						":",
						this.m_ListenPort
					}));
				}
				result = true;
			}
			return result;
		}

		public bool Listen(int serverListenPort)
		{
			return this.Listen(serverListenPort, this.m_HostTopology);
		}

		public bool Listen(int serverListenPort, HostTopology topology)
		{
			this.m_HostTopology = topology;
			this.Initialize();
			this.m_ListenPort = serverListenPort;
			if (this.m_UseWebSockets)
			{
				this.m_ServerHostId = NetworkTransport.AddWebsocketHost(this.m_HostTopology, serverListenPort);
			}
			else
			{
				this.m_ServerHostId = NetworkTransport.AddHost(this.m_HostTopology, serverListenPort);
			}
			bool result;
			if (this.m_ServerHostId == -1)
			{
				result = false;
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("NetworkServerSimple listen " + this.m_ListenPort);
				}
				result = true;
			}
			return result;
		}

		public void ListenRelay(string relayIp, int relayPort, NetworkID netGuid, SourceID sourceId, NodeID nodeId)
		{
			this.Initialize();
			this.m_ServerHostId = NetworkTransport.AddHost(this.m_HostTopology, this.listenPort);
			if (LogFilter.logDebug)
			{
				Debug.Log("Server Host Slot Id: " + this.m_ServerHostId);
			}
			this.Update();
			byte b;
			NetworkTransport.ConnectAsNetworkHost(this.m_ServerHostId, relayIp, relayPort, netGuid, sourceId, nodeId, out b);
			this.m_RelaySlotId = 0;
			if (LogFilter.logDebug)
			{
				Debug.Log("Relay Slot Id: " + this.m_RelaySlotId);
			}
		}

		public void Stop()
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkServerSimple stop ");
			}
			NetworkTransport.RemoveHost(this.m_ServerHostId);
			this.m_ServerHostId = -1;
		}

		internal void RegisterHandlerSafe(short msgType, QSBNetworkMessageDelegate handler)
		{
			this.m_MessageHandlers.RegisterHandlerSafe(msgType, handler);
		}

		public void RegisterHandler(short msgType, QSBNetworkMessageDelegate handler)
		{
			this.m_MessageHandlers.RegisterHandler(msgType, handler);
		}

		public void UnregisterHandler(short msgType)
		{
			this.m_MessageHandlers.UnregisterHandler(msgType);
		}

		public void ClearHandlers()
		{
			this.m_MessageHandlers.ClearMessageHandlers();
		}

		public void UpdateConnections()
		{
			for (int i = 0; i < this.m_Connections.Count; i++)
			{
				QSBNetworkConnection networkConnection = this.m_Connections[i];
				if (networkConnection != null)
				{
					networkConnection.FlushChannels();
				}
			}
		}

		public void Update()
		{
			if (this.m_ServerHostId != -1)
			{
				NetworkEventType networkEventType;
				if (this.m_RelaySlotId != -1)
				{
					byte b;
					networkEventType = NetworkTransport.ReceiveRelayEventFromHost(this.m_ServerHostId, out b);
					if (networkEventType != NetworkEventType.Nothing)
					{
						if (LogFilter.logDebug)
						{
							Debug.Log("NetGroup event:" + networkEventType);
						}
					}
					if (networkEventType == NetworkEventType.ConnectEvent)
					{
						if (LogFilter.logDebug)
						{
							Debug.Log("NetGroup server connected");
						}
					}
					if (networkEventType == NetworkEventType.DisconnectEvent)
					{
						if (LogFilter.logDebug)
						{
							Debug.Log("NetGroup server disconnected");
						}
					}
				}
				do
				{
					byte b;
					int connectionId;
					int channelId;
					int receivedSize;
					networkEventType = NetworkTransport.ReceiveFromHost(this.m_ServerHostId, out connectionId, out channelId, this.m_MsgBuffer, this.m_MsgBuffer.Length, out receivedSize, out b);
					if (networkEventType != NetworkEventType.Nothing)
					{
						Debug.Log(string.Concat(new object[]
						{
							"Server event: host=",
							this.m_ServerHostId,
							" event=",
							networkEventType,
							" error=",
							b
						}));
					}
					switch (networkEventType)
					{
						case NetworkEventType.DataEvent:
							this.HandleData(connectionId, channelId, receivedSize, b);
							break;

						case NetworkEventType.ConnectEvent:
							this.HandleConnect(connectionId, b);
							break;

						case NetworkEventType.DisconnectEvent:
							this.HandleDisconnect(connectionId, b);
							break;

						case NetworkEventType.Nothing:
							break;

						default:
							if (LogFilter.logError)
							{
								Debug.LogError("Unknown network message type received: " + networkEventType);
							}
							break;
					}
				}
				while (networkEventType != NetworkEventType.Nothing);
				this.UpdateConnections();
			}
		}

		public QSBNetworkConnection FindConnection(int connectionId)
		{
			QSBNetworkConnection result;
			if (connectionId < 0 || connectionId >= this.m_Connections.Count)
			{
				result = null;
			}
			else
			{
				result = this.m_Connections[connectionId];
			}
			return result;
		}

		public bool SetConnectionAtIndex(QSBNetworkConnection conn)
		{
			while (this.m_Connections.Count <= conn.connectionId)
			{
				this.m_Connections.Add(null);
			}
			bool result;
			if (this.m_Connections[conn.connectionId] != null)
			{
				result = false;
			}
			else
			{
				this.m_Connections[conn.connectionId] = conn;
				conn.SetHandlers(this.m_MessageHandlers);
				result = true;
			}
			return result;
		}

		public bool RemoveConnectionAtIndex(int connectionId)
		{
			bool result;
			if (connectionId < 0 || connectionId >= this.m_Connections.Count)
			{
				result = false;
			}
			else
			{
				this.m_Connections[connectionId] = null;
				result = true;
			}
			return result;
		}

		private void HandleConnect(int connectionId, byte error)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkServerSimple accepted client:" + connectionId);
			}
			if (error != 0)
			{
				this.OnConnectError(connectionId, error);
			}
			else
			{
				string networkAddress;
				int num;
				NetworkID networkID;
				NodeID nodeID;
				byte lastError;
				NetworkTransport.GetConnectionInfo(this.m_ServerHostId, connectionId, out networkAddress, out num, out networkID, out nodeID, out lastError);
				QSBNetworkConnection networkConnection = (QSBNetworkConnection)Activator.CreateInstance(this.m_NetworkConnectionClass);
				networkConnection.SetHandlers(this.m_MessageHandlers);
				networkConnection.Initialize(networkAddress, this.m_ServerHostId, connectionId, this.m_HostTopology);
				networkConnection.LastError = (NetworkError)lastError;
				while (this.m_Connections.Count <= connectionId)
				{
					this.m_Connections.Add(null);
				}
				this.m_Connections[connectionId] = networkConnection;
				this.OnConnected(networkConnection);
			}
		}

		private void HandleDisconnect(int connectionId, byte error)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkServerSimple disconnect client:" + connectionId);
			}
			QSBNetworkConnection networkConnection = this.FindConnection(connectionId);
			if (networkConnection != null)
			{
				networkConnection.LastError = (NetworkError)error;
				if (error != 0)
				{
					if (error != 6)
					{
						this.m_Connections[connectionId] = null;
						if (LogFilter.logError)
						{
							Debug.LogError(string.Concat(new object[]
							{
								"Server client disconnect error, connectionId: ",
								connectionId,
								" error: ",
								(NetworkError)error
							}));
						}
						this.OnDisconnectError(networkConnection, error);
						return;
					}
				}
				networkConnection.Disconnect();
				this.m_Connections[connectionId] = null;
				if (LogFilter.logDebug)
				{
					Debug.Log("Server lost client:" + connectionId);
				}
				this.OnDisconnected(networkConnection);
			}
		}

		private void HandleData(int connectionId, int channelId, int receivedSize, byte error)
		{
			QSBNetworkConnection networkConnection = this.FindConnection(connectionId);
			if (networkConnection == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("HandleData Unknown connectionId:" + connectionId);
				}
			}
			else
			{
				networkConnection.LastError = (NetworkError)error;
				if (error != 0)
				{
					this.OnDataError(networkConnection, error);
				}
				else
				{
					this.m_MsgReader.SeekZero();
					this.OnData(networkConnection, receivedSize, channelId);
				}
			}
		}

		public void SendBytesTo(int connectionId, byte[] bytes, int numBytes, int channelId)
		{
			QSBNetworkConnection networkConnection = this.FindConnection(connectionId);
			if (networkConnection != null)
			{
				networkConnection.SendBytes(bytes, numBytes, channelId);
			}
		}

		public void SendWriterTo(int connectionId, NetworkWriter writer, int channelId)
		{
			QSBNetworkConnection networkConnection = this.FindConnection(connectionId);
			if (networkConnection != null)
			{
				networkConnection.SendWriter(writer, channelId);
			}
		}

		public void Disconnect(int connectionId)
		{
			QSBNetworkConnection networkConnection = this.FindConnection(connectionId);
			if (networkConnection != null)
			{
				networkConnection.Disconnect();
				this.m_Connections[connectionId] = null;
			}
		}

		public void DisconnectAllConnections()
		{
			for (int i = 0; i < this.m_Connections.Count; i++)
			{
				QSBNetworkConnection networkConnection = this.m_Connections[i];
				if (networkConnection != null)
				{
					networkConnection.Disconnect();
					networkConnection.Dispose();
				}
			}
		}

		public virtual void OnConnectError(int connectionId, byte error)
		{
			Debug.LogError("OnConnectError error:" + error);
		}

		public virtual void OnDataError(QSBNetworkConnection conn, byte error)
		{
			Debug.LogError("OnDataError error:" + error);
		}

		public virtual void OnDisconnectError(QSBNetworkConnection conn, byte error)
		{
			Debug.LogError("OnDisconnectError error:" + error);
		}

		public virtual void OnConnected(QSBNetworkConnection conn)
		{
			conn.InvokeHandlerNoData(32);
		}

		public virtual void OnDisconnected(QSBNetworkConnection conn)
		{
			conn.InvokeHandlerNoData(33);
		}

		public virtual void OnData(QSBNetworkConnection conn, int receivedSize, int channelId)
		{
			conn.TransportReceive(this.m_MsgBuffer, receivedSize, channelId);
		}

		private bool m_Initialized = false;

		private int m_ListenPort;

		private int m_ServerHostId = -1;

		private int m_RelaySlotId = -1;

		private bool m_UseWebSockets;

		private byte[] m_MsgBuffer = null;

		private NetworkReader m_MsgReader = null;

		private Type m_NetworkConnectionClass = typeof(QSBNetworkConnection);

		private HostTopology m_HostTopology;

		private List<QSBNetworkConnection> m_Connections = new List<QSBNetworkConnection>();

		private ReadOnlyCollection<QSBNetworkConnection> m_ConnectionsReadOnly;

		private QSBNetworkMessageHandlers m_MessageHandlers = new QSBNetworkMessageHandlers();
	}
}