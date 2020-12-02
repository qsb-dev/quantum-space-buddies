using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
	public class QSBNetworkServer
	{
		private QSBNetworkServer()
		{
			NetworkTransport.Init();
			m_RemoveList = new HashSet<NetworkInstanceId>();
			m_ExternalConnections = new HashSet<int>();
			m_NetworkScene = new NetworkScene();
			m_SimpleServerSimple = new ServerSimpleWrapper(this);
		}

		public static List<QSBNetworkConnection> localConnections
		{
			get
			{
				return instance.m_LocalConnectionsFakeList;
			}
		}

		public static int listenPort
		{
			get
			{
				return instance.m_SimpleServerSimple.listenPort;
			}
		}

		public static int serverHostId
		{
			get
			{
				return instance.m_SimpleServerSimple.serverHostId;
			}
		}

		public static ReadOnlyCollection<QSBNetworkConnection> connections
		{
			get
			{
				return instance.m_SimpleServerSimple.connections;
			}
		}

		public static Dictionary<short, NetworkMessageDelegate> handlers
		{
			get
			{
				return instance.m_SimpleServerSimple.handlers;
			}
		}

		public static HostTopology hostTopology
		{
			get
			{
				return instance.m_SimpleServerSimple.hostTopology;
			}
		}

		public static Dictionary<NetworkInstanceId, QSBNetworkIdentity> objects
		{
			get
			{
				return instance.m_NetworkScene.localObjects;
			}
		}

		[Obsolete("Moved to NetworkMigrationManager")]
		public static bool sendPeerInfo
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public static bool dontListen
		{
			get
			{
				return m_DontListen;
			}
			set
			{
				m_DontListen = value;
			}
		}

		public static bool useWebSockets
		{
			get
			{
				return instance.m_SimpleServerSimple.useWebSockets;
			}
			set
			{
				instance.m_SimpleServerSimple.useWebSockets = value;
			}
		}

		internal static QSBNetworkServer instance
		{
			get
			{
				if (s_Instance == null)
				{
					object obj = s_Sync;
					lock (obj)
					{
						if (s_Instance == null)
						{
							s_Instance = new QSBNetworkServer();
						}
					}
				}
				return s_Instance;
			}
		}

		public static bool active
		{
			get
			{
				return NetworkServer.s_Active;
			}
		}

		public static bool localClientActive
		{
			get
			{
				return instance.m_LocalClientActive;
			}
		}

		public static int numChannels
		{
			get
			{
				return instance.m_SimpleServerSimple.hostTopology.DefaultConfig.ChannelCount;
			}
		}

		public static float maxDelay
		{
			get
			{
				return instance.m_MaxDelay;
			}
			set
			{
				instance.InternalSetMaxDelay(value);
			}
		}

		public static Type networkConnectionClass
		{
			get
			{
				return instance.m_SimpleServerSimple.networkConnectionClass;
			}
		}

		public static void SetNetworkConnectionClass<T>() where T : NetworkConnection
		{
			instance.m_SimpleServerSimple.SetNetworkConnectionClass<T>();
		}

		public static bool Configure(ConnectionConfig config, int maxConnections)
		{
			return instance.m_SimpleServerSimple.Configure(config, maxConnections);
		}

		public static bool Configure(HostTopology topology)
		{
			return instance.m_SimpleServerSimple.Configure(topology);
		}

		public static void Reset()
		{
			NetworkTransport.Shutdown();
			NetworkTransport.Init();
			s_Instance = null;
			s_Active = false;
		}

		public static void Shutdown()
		{
			if (s_Instance != null)
			{
				s_Instance.InternalDisconnectAll();
				if (!m_DontListen)
				{
					s_Instance.m_SimpleServerSimple.Stop();
				}
				s_Instance = null;
			}
			m_DontListen = false;
			s_Active = false;
		}

		public static bool Listen(MatchInfo matchInfo, int listenPort)
		{
			bool result;
			if (!matchInfo.usingRelay)
			{
				result = instance.InternalListen(null, listenPort);
			}
			else
			{
				instance.InternalListenRelay(matchInfo.address, matchInfo.port, matchInfo.networkId, Utility.GetSourceID(), matchInfo.nodeId);
				result = true;
			}
			return result;
		}

		internal void RegisterMessageHandlers()
		{
			NetworkServerSimple simpleServerSimple = m_SimpleServerSimple;
			short msgType = 35;
			if (NetworkServer.<> f__mg$cache0 == null)
			{
				NetworkServer.<> f__mg$cache0 = new NetworkMessageDelegate(NetworkServer.OnClientReadyMessage);
			}
			simpleServerSimple.RegisterHandlerSafe(msgType, NetworkServer.<> f__mg$cache0);
			NetworkServerSimple simpleServerSimple2 = m_SimpleServerSimple;
			short msgType2 = 5;
			if (NetworkServer.<> f__mg$cache1 == null)
			{
				NetworkServer.<> f__mg$cache1 = new NetworkMessageDelegate(NetworkServer.OnCommandMessage);
			}
			simpleServerSimple2.RegisterHandlerSafe(msgType2, NetworkServer.<> f__mg$cache1);
			NetworkServerSimple simpleServerSimple3 = m_SimpleServerSimple;
			short msgType3 = 6;
			if (NetworkServer.<> f__mg$cache2 == null)
			{
				NetworkServer.<> f__mg$cache2 = new NetworkMessageDelegate(NetworkTransform.HandleTransform);
			}
			simpleServerSimple3.RegisterHandlerSafe(msgType3, NetworkServer.<> f__mg$cache2);
			NetworkServerSimple simpleServerSimple4 = m_SimpleServerSimple;
			short msgType4 = 16;
			if (NetworkServer.<> f__mg$cache3 == null)
			{
				NetworkServer.<> f__mg$cache3 = new NetworkMessageDelegate(NetworkTransformChild.HandleChildTransform);
			}
			simpleServerSimple4.RegisterHandlerSafe(msgType4, NetworkServer.<> f__mg$cache3);
			NetworkServerSimple simpleServerSimple5 = m_SimpleServerSimple;
			short msgType5 = 38;
			if (NetworkServer.<> f__mg$cache4 == null)
			{
				NetworkServer.<> f__mg$cache4 = new NetworkMessageDelegate(NetworkServer.OnRemovePlayerMessage);
			}
			simpleServerSimple5.RegisterHandlerSafe(msgType5, NetworkServer.<> f__mg$cache4);
			NetworkServerSimple simpleServerSimple6 = m_SimpleServerSimple;
			short msgType6 = 40;
			if (NetworkServer.<> f__mg$cache5 == null)
			{
				NetworkServer.<> f__mg$cache5 = new NetworkMessageDelegate(NetworkAnimator.OnAnimationServerMessage);
			}
			simpleServerSimple6.RegisterHandlerSafe(msgType6, NetworkServer.<> f__mg$cache5);
			NetworkServerSimple simpleServerSimple7 = m_SimpleServerSimple;
			short msgType7 = 41;
			if (NetworkServer.<> f__mg$cache6 == null)
			{
				NetworkServer.<> f__mg$cache6 = new NetworkMessageDelegate(NetworkAnimator.OnAnimationParametersServerMessage);
			}
			simpleServerSimple7.RegisterHandlerSafe(msgType7, NetworkServer.<> f__mg$cache6);
			NetworkServerSimple simpleServerSimple8 = m_SimpleServerSimple;
			short msgType8 = 42;
			if (NetworkServer.<> f__mg$cache7 == null)
			{
				NetworkServer.<> f__mg$cache7 = new NetworkMessageDelegate(NetworkAnimator.OnAnimationTriggerServerMessage);
			}
			simpleServerSimple8.RegisterHandlerSafe(msgType8, NetworkServer.<> f__mg$cache7);
			NetworkServerSimple simpleServerSimple9 = m_SimpleServerSimple;
			short msgType9 = 17;
			if (NetworkServer.<> f__mg$cache8 == null)
			{
				NetworkServer.<> f__mg$cache8 = new NetworkMessageDelegate(NetworkConnection.OnFragment);
			}
			simpleServerSimple9.RegisterHandlerSafe(msgType9, NetworkServer.<> f__mg$cache8);
			NetworkServer.maxPacketSize = NetworkServer.hostTopology.DefaultConfig.PacketSize;
		}

		public static void ListenRelay(string relayIp, int relayPort, NetworkID netGuid, SourceID sourceId, NodeID nodeId)
		{
			instance.InternalListenRelay(relayIp, relayPort, netGuid, sourceId, nodeId);
		}

		private void InternalListenRelay(string relayIp, int relayPort, NetworkID netGuid, SourceID sourceId, NodeID nodeId)
		{
			m_SimpleServerSimple.ListenRelay(relayIp, relayPort, netGuid, sourceId, nodeId);
			s_Active = true;
			RegisterMessageHandlers();
		}

		public static bool Listen(int serverPort)
		{
			return instance.InternalListen(null, serverPort);
		}

		public static bool Listen(string ipAddress, int serverPort)
		{
			return instance.InternalListen(ipAddress, serverPort);
		}

		internal bool InternalListen(string ipAddress, int serverPort)
		{
			if (m_DontListen)
			{
				m_SimpleServerSimple.Initialize();
			}
			else if (!m_SimpleServerSimple.Listen(ipAddress, serverPort))
			{
				return false;
			}
			maxPacketSize = NetworkServer.hostTopology.DefaultConfig.PacketSize;
			s_Active = true;
			RegisterMessageHandlers();
			return true;
		}

		public static QSBNetworkClient BecomeHost(QSBNetworkClient oldClient, int port, MatchInfo matchInfo, int oldConnectionId, PeerInfoMessage[] peers)
		{
			return instance.BecomeHostInternal(oldClient, port, matchInfo, oldConnectionId, peers);
		}

		internal QSBNetworkClient BecomeHostInternal(QSBNetworkClient oldClient, int port, MatchInfo matchInfo, int oldConnectionId, PeerInfoMessage[] peers)
		{
			NetworkClient result;
			if (NetworkServer.s_Active)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("BecomeHost already a server.");
				}
				result = null;
			}
			else if (!NetworkClient.active)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("BecomeHost NetworkClient not active.");
				}
				result = null;
			}
			else
			{
				NetworkServer.Configure(NetworkServer.hostTopology);
				if (matchInfo == null)
				{
					if (LogFilter.logDev)
					{
						Debug.Log("BecomeHost Listen on " + port);
					}
					if (!NetworkServer.Listen(port))
					{
						if (LogFilter.logError)
						{
							Debug.LogError("BecomeHost bind failed.");
						}
						return null;
					}
				}
				else
				{
					if (LogFilter.logDev)
					{
						Debug.Log("BecomeHost match:" + matchInfo.networkId);
					}
					NetworkServer.ListenRelay(matchInfo.address, matchInfo.port, matchInfo.networkId, Utility.GetSourceID(), matchInfo.nodeId);
				}
				foreach (NetworkIdentity networkIdentity in ClientScene.objects.Values)
				{
					if (!(networkIdentity == null) && !(networkIdentity.gameObject == null))
					{
						NetworkIdentity.AddNetworkId(networkIdentity.netId.Value);
						m_NetworkScene.SetLocalObject(networkIdentity.netId, networkIdentity.gameObject, false, false);
						networkIdentity.OnStartServer(true);
					}
				}
				if (LogFilter.logDev)
				{
					Debug.Log("NetworkServer BecomeHost done. oldConnectionId:" + oldConnectionId);
				}
				RegisterMessageHandlers();
				if (!NetworkClient.RemoveClient(oldClient))
				{
					if (LogFilter.logError)
					{
						Debug.LogError("BecomeHost failed to remove client");
					}
				}
				if (LogFilter.logDev)
				{
					Debug.Log("BecomeHost localClient ready");
				}
				NetworkClient networkClient = ClientScene.ReconnectLocalServer();
				ClientScene.Ready(networkClient.connection);
				ClientScene.SetReconnectId(oldConnectionId, peers);
				ClientScene.AddPlayer(ClientScene.readyConnection, 0);
				result = networkClient;
			}
			return result;
		}

		private void InternalSetMaxDelay(float seconds)
		{
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					networkConnection.SetMaxDelay(seconds);
				}
			}
			m_MaxDelay = seconds;
		}

		internal int AddLocalClient(QSBLocalClient localClient)
		{
			int result;
			if (m_LocalConnectionsFakeList.Count != 0)
			{
				Debug.LogError("Local Connection already exists");
				result = -1;
			}
			else
			{
				m_LocalConnection = new ULocalConnectionToClient(localClient);
				m_LocalConnection.connectionId = 0;
				m_SimpleServerSimple.SetConnectionAtIndex(m_LocalConnection);
				m_LocalConnectionsFakeList.Add(m_LocalConnection);
				m_LocalConnection.InvokeHandlerNoData(32);
				result = 0;
			}
			return result;
		}

		internal void RemoveLocalClient(QSBNetworkConnection localClientConnection)
		{
			for (int i = 0; i < m_LocalConnectionsFakeList.Count; i++)
			{
				if (m_LocalConnectionsFakeList[i].connectionId == localClientConnection.connectionId)
				{
					m_LocalConnectionsFakeList.RemoveAt(i);
					break;
				}
			}
			if (m_LocalConnection != null)
			{
				m_LocalConnection.Disconnect();
				m_LocalConnection.Dispose();
				m_LocalConnection = null;
			}
			m_LocalClientActive = false;
			m_SimpleServerSimple.RemoveConnectionAtIndex(0);
		}

		internal void SetLocalObjectOnServer(NetworkInstanceId netId, GameObject obj)
		{
			if (LogFilter.logDev)
			{
				Debug.Log(string.Concat(new object[]
				{
					"SetLocalObjectOnServer ",
					netId,
					" ",
					obj
				}));
			}
			m_NetworkScene.SetLocalObject(netId, obj, false, true);
		}

		internal void ActivateLocalClientScene()
		{
			if (!m_LocalClientActive)
			{
				m_LocalClientActive = true;
				foreach (NetworkIdentity networkIdentity in NetworkServer.objects.Values)
				{
					if (!networkIdentity.isClient)
					{
						if (LogFilter.logDev)
						{
							Debug.Log(string.Concat(new object[]
							{
								"ActivateClientScene ",
								networkIdentity.netId,
								" ",
								networkIdentity.gameObject
							}));
						}
						ClientScene.SetLocalObject(networkIdentity.netId, networkIdentity.gameObject);
						networkIdentity.OnStartClient();
					}
				}
			}
		}

		public static bool SendToAll(short msgType, MessageBase msg)
		{
			if (LogFilter.logDev)
			{
				Debug.Log("Server.SendToAll msgType:" + msgType);
			}
			bool flag = true;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					flag &= networkConnection.Send(msgType, msg);
				}
			}
			return flag;
		}

		private static bool SendToObservers(GameObject contextObj, short msgType, MessageBase msg)
		{
			if (LogFilter.logDev)
			{
				Debug.Log("Server.SendToObservers id:" + msgType);
			}
			bool flag = true;
			NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
			bool result;
			if (component == null || component.observers == null)
			{
				result = false;
			}
			else
			{
				int count = component.observers.Count;
				for (int i = 0; i < count; i++)
				{
					NetworkConnection networkConnection = component.observers[i];
					flag &= networkConnection.Send(msgType, msg);
				}
				result = flag;
			}
			return result;
		}

		public static bool SendToReady(GameObject contextObj, short msgType, MessageBase msg)
		{
			if (LogFilter.logDev)
			{
				Debug.Log("Server.SendToReady id:" + msgType);
			}
			bool result;
			if (contextObj == null)
			{
				for (int i = 0; i < NetworkServer.connections.Count; i++)
				{
					NetworkConnection networkConnection = NetworkServer.connections[i];
					if (networkConnection != null && networkConnection.isReady)
					{
						networkConnection.Send(msgType, msg);
					}
				}
				result = true;
			}
			else
			{
				bool flag = true;
				NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
				if (component == null || component.observers == null)
				{
					result = false;
				}
				else
				{
					int count = component.observers.Count;
					for (int j = 0; j < count; j++)
					{
						NetworkConnection networkConnection2 = component.observers[j];
						if (networkConnection2.isReady)
						{
							flag &= networkConnection2.Send(msgType, msg);
						}
					}
					result = flag;
				}
			}
			return result;
		}

		public static void SendWriterToReady(GameObject contextObj, NetworkWriter writer, int channelId)
		{
			if (writer.AsArraySegment().Count > 32767)
			{
				throw new UnityException("NetworkWriter used buffer is too big!");
			}
			NetworkServer.SendBytesToReady(contextObj, writer.AsArraySegment().Array, writer.AsArraySegment().Count, channelId);
		}

		public static void SendBytesToReady(GameObject contextObj, byte[] buffer, int numBytes, int channelId)
		{
			if (contextObj == null)
			{
				bool flag = true;
				for (int i = 0; i < NetworkServer.connections.Count; i++)
				{
					NetworkConnection networkConnection = NetworkServer.connections[i];
					if (networkConnection != null && networkConnection.isReady)
					{
						if (!networkConnection.SendBytes(buffer, numBytes, channelId))
						{
							flag = false;
						}
					}
				}
				if (!flag)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("SendBytesToReady failed");
					}
				}
			}
			else
			{
				NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
				try
				{
					bool flag2 = true;
					int count = component.observers.Count;
					for (int j = 0; j < count; j++)
					{
						NetworkConnection networkConnection2 = component.observers[j];
						if (networkConnection2.isReady)
						{
							if (!networkConnection2.SendBytes(buffer, numBytes, channelId))
							{
								flag2 = false;
							}
						}
					}
					if (!flag2)
					{
						if (LogFilter.logWarn)
						{
							Debug.LogWarning("SendBytesToReady failed for " + contextObj);
						}
					}
				}
				catch (NullReferenceException)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("SendBytesToReady object " + contextObj + " has not been spawned");
					}
				}
			}
		}

		public static void SendBytesToPlayer(GameObject player, byte[] buffer, int numBytes, int channelId)
		{
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					for (int j = 0; j < networkConnection.playerControllers.Count; j++)
					{
						if (networkConnection.playerControllers[j].IsValid && networkConnection.playerControllers[j].gameObject == player)
						{
							networkConnection.SendBytes(buffer, numBytes, channelId);
							break;
						}
					}
				}
			}
		}

		public static bool SendUnreliableToAll(short msgType, MessageBase msg)
		{
			if (LogFilter.logDev)
			{
				Debug.Log("Server.SendUnreliableToAll msgType:" + msgType);
			}
			bool flag = true;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					flag &= networkConnection.SendUnreliable(msgType, msg);
				}
			}
			return flag;
		}

		public static bool SendUnreliableToReady(GameObject contextObj, short msgType, MessageBase msg)
		{
			if (LogFilter.logDev)
			{
				Debug.Log("Server.SendUnreliableToReady id:" + msgType);
			}
			bool result;
			if (contextObj == null)
			{
				for (int i = 0; i < NetworkServer.connections.Count; i++)
				{
					NetworkConnection networkConnection = NetworkServer.connections[i];
					if (networkConnection != null && networkConnection.isReady)
					{
						networkConnection.SendUnreliable(msgType, msg);
					}
				}
				result = true;
			}
			else
			{
				bool flag = true;
				NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
				int count = component.observers.Count;
				for (int j = 0; j < count; j++)
				{
					NetworkConnection networkConnection2 = component.observers[j];
					if (networkConnection2.isReady)
					{
						flag &= networkConnection2.SendUnreliable(msgType, msg);
					}
				}
				result = flag;
			}
			return result;
		}

		public static bool SendByChannelToAll(short msgType, MessageBase msg, int channelId)
		{
			if (LogFilter.logDev)
			{
				Debug.Log("Server.SendByChannelToAll id:" + msgType);
			}
			bool flag = true;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					flag &= networkConnection.SendByChannel(msgType, msg, channelId);
				}
			}
			return flag;
		}

		public static bool SendByChannelToReady(GameObject contextObj, short msgType, MessageBase msg, int channelId)
		{
			if (LogFilter.logDev)
			{
				Debug.Log("Server.SendByChannelToReady msgType:" + msgType);
			}
			bool result;
			if (contextObj == null)
			{
				for (int i = 0; i < NetworkServer.connections.Count; i++)
				{
					NetworkConnection networkConnection = NetworkServer.connections[i];
					if (networkConnection != null && networkConnection.isReady)
					{
						networkConnection.SendByChannel(msgType, msg, channelId);
					}
				}
				result = true;
			}
			else
			{
				bool flag = true;
				NetworkIdentity component = contextObj.GetComponent<NetworkIdentity>();
				int count = component.observers.Count;
				for (int j = 0; j < count; j++)
				{
					NetworkConnection networkConnection2 = component.observers[j];
					if (networkConnection2.isReady)
					{
						flag &= networkConnection2.SendByChannel(msgType, msg, channelId);
					}
				}
				result = flag;
			}
			return result;
		}

		public static void DisconnectAll()
		{
			instance.InternalDisconnectAll();
		}

		internal void InternalDisconnectAll()
		{
			m_SimpleServerSimple.DisconnectAllConnections();
			if (m_LocalConnection != null)
			{
				m_LocalConnection.Disconnect();
				m_LocalConnection.Dispose();
				m_LocalConnection = null;
			}
			m_LocalClientActive = false;
		}

		internal static void Update()
		{
			if (NetworkServer.s_Instance != null)
			{
				NetworkServer.s_Instance.InternalUpdate();
			}
		}

		private void UpdateServerObjects()
		{
			foreach (NetworkIdentity networkIdentity in NetworkServer.objects.Values)
			{
				try
				{
					networkIdentity.UNetUpdate();
				}
				catch (NullReferenceException)
				{
				}
				catch (MissingReferenceException)
				{
				}
			}
			if (m_RemoveListCount++ % 100 == 0)
			{
				CheckForNullObjects();
			}
		}

		private void CheckForNullObjects()
		{
			foreach (NetworkInstanceId networkInstanceId in NetworkServer.objects.Keys)
			{
				NetworkIdentity networkIdentity = NetworkServer.objects[networkInstanceId];
				if (networkIdentity == null || networkIdentity.gameObject == null)
				{
					m_RemoveList.Add(networkInstanceId);
				}
			}
			if (m_RemoveList.Count > 0)
			{
				foreach (NetworkInstanceId key in m_RemoveList)
				{
					NetworkServer.objects.Remove(key);
				}
				m_RemoveList.Clear();
			}
		}

		internal void InternalUpdate()
		{
			m_SimpleServerSimple.Update();
			if (NetworkServer.m_DontListen)
			{
				m_SimpleServerSimple.UpdateConnections();
			}
			UpdateServerObjects();
		}

		private void OnConnected(NetworkConnection conn)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("Server accepted client:" + conn.connectionId);
			}
			conn.SetMaxDelay(m_MaxDelay);
			conn.InvokeHandlerNoData(32);
			NetworkServer.SendCrc(conn);
		}

		private void OnDisconnected(NetworkConnection conn)
		{
			conn.InvokeHandlerNoData(33);
			for (int i = 0; i < conn.playerControllers.Count; i++)
			{
				if (conn.playerControllers[i].gameObject != null)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("Player not destroyed when connection disconnected.");
					}
				}
			}
			if (LogFilter.logDebug)
			{
				Debug.Log("Server lost client:" + conn.connectionId);
			}
			conn.RemoveObservers();
			conn.Dispose();
		}

		private void OnData(NetworkConnection conn, int receivedSize, int channelId)
		{
			conn.TransportReceive(m_SimpleServerSimple.messageBuffer, receivedSize, channelId);
		}

		private void GenerateConnectError(int error)
		{
			if (LogFilter.logError)
			{
				Debug.LogError("UNet Server Connect Error: " + error);
			}
			GenerateError(null, error);
		}

		private void GenerateDataError(NetworkConnection conn, int error)
		{
			if (LogFilter.logError)
			{
				Debug.LogError("UNet Server Data Error: " + (NetworkError)error);
			}
			GenerateError(conn, error);
		}

		private void GenerateDisconnectError(NetworkConnection conn, int error)
		{
			if (LogFilter.logError)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"UNet Server Disconnect Error: ",
					(NetworkError)error,
					" conn:[",
					conn,
					"]:",
					conn.connectionId
				}));
			}
			GenerateError(conn, error);
		}

		private void GenerateError(NetworkConnection conn, int error)
		{
			if (NetworkServer.handlers.ContainsKey(34))
			{
				ErrorMessage errorMessage = new ErrorMessage();
				errorMessage.errorCode = error;
				NetworkWriter writer = new NetworkWriter();
				errorMessage.Serialize(writer);
				NetworkReader reader = new NetworkReader(writer);
				conn.InvokeHandler(34, reader, 0);
			}
		}

		public static void RegisterHandler(short msgType, QSBNetworkMessageDelegate handler)
		{
			instance.m_SimpleServerSimple.RegisterHandler(msgType, handler);
		}

		public static void UnregisterHandler(short msgType)
		{
			instance.m_SimpleServerSimple.UnregisterHandler(msgType);
		}

		public static void ClearHandlers()
		{
			instance.m_SimpleServerSimple.ClearHandlers();
		}

		public static void ClearSpawners()
		{
			NetworkScene.ClearSpawners();
		}

		public static void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
		{
			numMsgs = 0;
			numBufferedMsgs = 0;
			numBytes = 0;
			lastBufferedPerSecond = 0;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					int num;
					int num2;
					int num3;
					int num4;
					networkConnection.GetStatsOut(out num, out num2, out num3, out num4);
					numMsgs += num;
					numBufferedMsgs += num2;
					numBytes += num3;
					lastBufferedPerSecond += num4;
				}
			}
		}

		public static void GetStatsIn(out int numMsgs, out int numBytes)
		{
			numMsgs = 0;
			numBytes = 0;
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					int num;
					int num2;
					networkConnection.GetStatsIn(out num, out num2);
					numMsgs += num;
					numBytes += num2;
				}
			}
		}

		public static void SendToClientOfPlayer(GameObject player, short msgType, MessageBase msg)
		{
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					for (int j = 0; j < networkConnection.playerControllers.Count; j++)
					{
						if (networkConnection.playerControllers[j].IsValid && networkConnection.playerControllers[j].gameObject == player)
						{
							networkConnection.Send(msgType, msg);
							return;
						}
					}
				}
			}
			if (LogFilter.logError)
			{
				Debug.LogError("Failed to send message to player object '" + player.name + ", not found in connection list");
				return;
			}
		}

		public static void SendToClient(int connectionId, short msgType, MessageBase msg)
		{
			if (connectionId < NetworkServer.connections.Count)
			{
				NetworkConnection networkConnection = NetworkServer.connections[connectionId];
				if (networkConnection != null)
				{
					networkConnection.Send(msgType, msg);
					return;
				}
			}
			if (LogFilter.logError)
			{
				Debug.LogError("Failed to send message to connection ID '" + connectionId + ", not found in connection list");
			}
		}

		public static bool ReplacePlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId, NetworkHash128 assetId)
		{
			NetworkIdentity networkIdentity;
			if (NetworkServer.GetNetworkIdentity(player, out networkIdentity))
			{
				networkIdentity.SetDynamicAssetId(assetId);
			}
			return instance.InternalReplacePlayerForConnection(conn, player, playerControllerId);
		}

		public static bool ReplacePlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId)
		{
			return instance.InternalReplacePlayerForConnection(conn, player, playerControllerId);
		}

		public static bool AddPlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId, NetworkHash128 assetId)
		{
			NetworkIdentity networkIdentity;
			if (NetworkServer.GetNetworkIdentity(player, out networkIdentity))
			{
				networkIdentity.SetDynamicAssetId(assetId);
			}
			return instance.InternalAddPlayerForConnection(conn, player, playerControllerId);
		}

		public static bool AddPlayerForConnection(NetworkConnection conn, GameObject player, short playerControllerId)
		{
			return instance.InternalAddPlayerForConnection(conn, player, playerControllerId);
		}

		internal bool InternalAddPlayerForConnection(NetworkConnection conn, GameObject playerGameObject, short playerControllerId)
		{
			NetworkIdentity networkIdentity;
			bool result;
			if (!NetworkServer.GetNetworkIdentity(playerGameObject, out networkIdentity))
			{
				if (LogFilter.logError)
				{
					Debug.Log("AddPlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + playerGameObject);
				}
				result = false;
			}
			else
			{
				networkIdentity.Reset();
				if (!NetworkServer.CheckPlayerControllerIdForConnection(conn, playerControllerId))
				{
					result = false;
				}
				else
				{
					PlayerController playerController = null;
					GameObject x = null;
					if (conn.GetPlayerController(playerControllerId, out playerController))
					{
						x = playerController.gameObject;
					}
					if (x != null)
					{
						if (LogFilter.logError)
						{
							Debug.Log("AddPlayer: player object already exists for playerControllerId of " + playerControllerId);
						}
						result = false;
					}
					else
					{
						PlayerController playerController2 = new PlayerController(playerGameObject, playerControllerId);
						conn.SetPlayerController(playerController2);
						networkIdentity.SetConnectionToClient(conn, playerController2.playerControllerId);
						NetworkServer.SetClientReady(conn);
						if (SetupLocalPlayerForConnection(conn, networkIdentity, playerController2))
						{
							result = true;
						}
						else
						{
							if (LogFilter.logDebug)
							{
								Debug.Log(string.Concat(new object[]
								{
									"Adding new playerGameObject object netId: ",
									playerGameObject.GetComponent<NetworkIdentity>().netId,
									" asset ID ",
									playerGameObject.GetComponent<NetworkIdentity>().assetId
								}));
							}
							NetworkServer.FinishPlayerForConnection(conn, networkIdentity, playerGameObject);
							if (networkIdentity.localPlayerAuthority)
							{
								networkIdentity.SetClientOwner(conn);
							}
							result = true;
						}
					}
				}
			}
			return result;
		}

		private static bool CheckPlayerControllerIdForConnection(NetworkConnection conn, short playerControllerId)
		{
			bool result;
			if (playerControllerId < 0)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("AddPlayer: playerControllerId of " + playerControllerId + " is negative");
				}
				result = false;
			}
			else if (playerControllerId > 32)
			{
				if (LogFilter.logError)
				{
					Debug.Log(string.Concat(new object[]
					{
						"AddPlayer: playerControllerId of ",
						playerControllerId,
						" is too high. max is ",
						32
					}));
				}
				result = false;
			}
			else
			{
				if (playerControllerId > 16)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("AddPlayer: playerControllerId of " + playerControllerId + " is unusually high");
					}
				}
				result = true;
			}
			return result;
		}

		private bool SetupLocalPlayerForConnection(NetworkConnection conn, NetworkIdentity uv, PlayerController newPlayerController)
		{
			if (LogFilter.logDev)
			{
				Debug.Log("NetworkServer SetupLocalPlayerForConnection netID:" + uv.netId);
			}
			ULocalConnectionToClient ulocalConnectionToClient = conn as ULocalConnectionToClient;
			bool result;
			if (ulocalConnectionToClient != null)
			{
				if (LogFilter.logDev)
				{
					Debug.Log("NetworkServer AddPlayer handling ULocalConnectionToClient");
				}
				if (uv.netId.IsEmpty())
				{
					uv.OnStartServer(true);
				}
				uv.RebuildObservers(true);
				SendSpawnMessage(uv, null);
				ulocalConnectionToClient.localClient.AddLocalPlayer(newPlayerController);
				uv.SetClientOwner(conn);
				uv.ForceAuthority(true);
				uv.SetLocalPlayer(newPlayerController.playerControllerId);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		private static void FinishPlayerForConnection(NetworkConnection conn, NetworkIdentity uv, GameObject playerGameObject)
		{
			if (uv.netId.IsEmpty())
			{
				NetworkServer.Spawn(playerGameObject);
			}
			conn.Send(4, new OwnerMessage
			{
				netId = uv.netId,
				playerControllerId = uv.playerControllerId
			});
		}

		internal bool InternalReplacePlayerForConnection(NetworkConnection conn, GameObject playerGameObject, short playerControllerId)
		{
			NetworkIdentity networkIdentity;
			bool result;
			if (!NetworkServer.GetNetworkIdentity(playerGameObject, out networkIdentity))
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ReplacePlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + playerGameObject);
				}
				result = false;
			}
			else if (!NetworkServer.CheckPlayerControllerIdForConnection(conn, playerControllerId))
			{
				result = false;
			}
			else
			{
				if (LogFilter.logDev)
				{
					Debug.Log("NetworkServer ReplacePlayer");
				}
				PlayerController playerController;
				if (conn.GetPlayerController(playerControllerId, out playerController))
				{
					playerController.unetView.SetNotLocalPlayer();
					playerController.unetView.ClearClientOwner();
				}
				PlayerController playerController2 = new PlayerController(playerGameObject, playerControllerId);
				conn.SetPlayerController(playerController2);
				networkIdentity.SetConnectionToClient(conn, playerController2.playerControllerId);
				if (LogFilter.logDev)
				{
					Debug.Log("NetworkServer ReplacePlayer setup local");
				}
				if (SetupLocalPlayerForConnection(conn, networkIdentity, playerController2))
				{
					result = true;
				}
				else
				{
					if (LogFilter.logDebug)
					{
						Debug.Log(string.Concat(new object[]
						{
							"Replacing playerGameObject object netId: ",
							playerGameObject.GetComponent<NetworkIdentity>().netId,
							" asset ID ",
							playerGameObject.GetComponent<NetworkIdentity>().assetId
						}));
					}
					NetworkServer.FinishPlayerForConnection(conn, networkIdentity, playerGameObject);
					if (networkIdentity.localPlayerAuthority)
					{
						networkIdentity.SetClientOwner(conn);
					}
					result = true;
				}
			}
			return result;
		}

		private static bool GetNetworkIdentity(GameObject go, out NetworkIdentity view)
		{
			view = go.GetComponent<NetworkIdentity>();
			bool result;
			if (view == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("UNET failure. GameObject doesn't have NetworkIdentity.");
				}
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		public static void SetClientReady(NetworkConnection conn)
		{
			instance.SetClientReadyInternal(conn);
		}

		internal void SetClientReadyInternal(NetworkConnection conn)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("SetClientReadyInternal for conn:" + conn.connectionId);
			}
			if (conn.isReady)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("SetClientReady conn " + conn.connectionId + " already ready");
				}
			}
			else
			{
				if (conn.playerControllers.Count == 0)
				{
					if (LogFilter.logDebug)
					{
						Debug.LogWarning("Ready with no player object");
					}
				}
				conn.isReady = true;
				ULocalConnectionToClient ulocalConnectionToClient = conn as ULocalConnectionToClient;
				if (ulocalConnectionToClient != null)
				{
					if (LogFilter.logDev)
					{
						Debug.Log("NetworkServer Ready handling ULocalConnectionToClient");
					}
					foreach (NetworkIdentity networkIdentity in NetworkServer.objects.Values)
					{
						if (networkIdentity != null && networkIdentity.gameObject != null)
						{
							bool flag = networkIdentity.OnCheckObserver(conn);
							if (flag)
							{
								networkIdentity.AddObserver(conn);
							}
							if (!networkIdentity.isClient)
							{
								if (LogFilter.logDev)
								{
									Debug.Log("LocalClient.SetSpawnObject calling OnStartClient");
								}
								networkIdentity.OnStartClient();
							}
						}
					}
				}
				else
				{
					if (LogFilter.logDebug)
					{
						Debug.Log(string.Concat(new object[]
						{
							"Spawning ",
							NetworkServer.objects.Count,
							" objects for conn ",
							conn.connectionId
						}));
					}
					ObjectSpawnFinishedMessage objectSpawnFinishedMessage = new ObjectSpawnFinishedMessage();
					objectSpawnFinishedMessage.state = 0U;
					conn.Send(12, objectSpawnFinishedMessage);
					foreach (NetworkIdentity networkIdentity2 in NetworkServer.objects.Values)
					{
						if (networkIdentity2 == null)
						{
							if (LogFilter.logWarn)
							{
								Debug.LogWarning("Invalid object found in server local object list (null NetworkIdentity).");
							}
						}
						else if (networkIdentity2.gameObject.activeSelf)
						{
							if (LogFilter.logDebug)
							{
								Debug.Log(string.Concat(new object[]
								{
									"Sending spawn message for current server objects name='",
									networkIdentity2.gameObject.name,
									"' netId=",
									networkIdentity2.netId
								}));
							}
							bool flag2 = networkIdentity2.OnCheckObserver(conn);
							if (flag2)
							{
								networkIdentity2.AddObserver(conn);
							}
						}
					}
					objectSpawnFinishedMessage.state = 1U;
					conn.Send(12, objectSpawnFinishedMessage);
				}
			}
		}

		internal static void ShowForConnection(NetworkIdentity uv, NetworkConnection conn)
		{
			if (conn.isReady)
			{
				instance.SendSpawnMessage(uv, conn);
			}
		}

		internal static void HideForConnection(QSBNetworkIdentity uv, QSBNetworkConnection conn)
		{
			conn.Send(13, new QSBObjectDestroyMessage
			{
				netId = uv.NetId
			});
		}

		public static void SetAllClientsNotReady()
		{
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					NetworkServer.SetClientNotReady(networkConnection);
				}
			}
		}

		public static void SetClientNotReady(NetworkConnection conn)
		{
			instance.InternalSetClientNotReady(conn);
		}

		internal void InternalSetClientNotReady(NetworkConnection conn)
		{
			if (conn.isReady)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("PlayerNotReady " + conn);
				}
				conn.isReady = false;
				conn.RemoveObservers();
				NotReadyMessage msg = new NotReadyMessage();
				conn.Send(36, msg);
			}
		}

		private static void OnClientReadyMessage(NetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("Default handler for ready message from " + netMsg.conn);
			}
			NetworkServer.SetClientReady(netMsg.conn);
		}

		private static void OnRemovePlayerMessage(NetworkMessage netMsg)
		{
			netMsg.ReadMessage<RemovePlayerMessage>(NetworkServer.s_RemovePlayerMessage);
			PlayerController playerController = null;
			netMsg.conn.GetPlayerController(NetworkServer.s_RemovePlayerMessage.playerControllerId, out playerController);
			if (playerController != null)
			{
				netMsg.conn.RemovePlayerController(NetworkServer.s_RemovePlayerMessage.playerControllerId);
				NetworkServer.Destroy(playerController.gameObject);
			}
			else if (LogFilter.logError)
			{
				Debug.LogError("Received remove player message but could not find the player ID: " + NetworkServer.s_RemovePlayerMessage.playerControllerId);
			}
		}

		private static void OnCommandMessage(NetworkMessage netMsg)
		{
			int cmdHash = (int)netMsg.reader.ReadPackedUInt32();
			NetworkInstanceId networkInstanceId = netMsg.reader.ReadNetworkId();
			GameObject gameObject = NetworkServer.FindLocalObject(networkInstanceId);
			if (gameObject == null)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("Instance not found when handling Command message [netId=" + networkInstanceId + "]");
				}
			}
			else
			{
				NetworkIdentity component = gameObject.GetComponent<NetworkIdentity>();
				if (component == null)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("NetworkIdentity deleted when handling Command message [netId=" + networkInstanceId + "]");
					}
				}
				else
				{
					bool flag = false;
					for (int i = 0; i < netMsg.conn.playerControllers.Count; i++)
					{
						PlayerController playerController = netMsg.conn.playerControllers[i];
						if (playerController.gameObject != null && playerController.gameObject.GetComponent<NetworkIdentity>().netId == component.netId)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						if (component.clientAuthorityOwner != netMsg.conn)
						{
							if (LogFilter.logWarn)
							{
								Debug.LogWarning("Command for object without authority [netId=" + networkInstanceId + "]");
							}
							return;
						}
					}
					if (LogFilter.logDev)
					{
						Debug.Log(string.Concat(new object[]
						{
							"OnCommandMessage for netId=",
							networkInstanceId,
							" conn=",
							netMsg.conn
						}));
					}
					component.HandleCommand(cmdHash, netMsg.reader);
				}
			}
		}

		internal void SpawnObject(GameObject obj)
		{
			NetworkIdentity networkIdentity;
			if (!NetworkServer.active)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("SpawnObject for " + obj + ", NetworkServer is not active. Cannot spawn objects without an active server.");
				}
			}
			else if (!NetworkServer.GetNetworkIdentity(obj, out networkIdentity))
			{
				if (LogFilter.logError)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"SpawnObject ",
						obj,
						" has no NetworkIdentity. Please add a NetworkIdentity to ",
						obj
					}));
				}
			}
			else
			{
				networkIdentity.Reset();
				networkIdentity.OnStartServer(false);
				if (LogFilter.logDebug)
				{
					Debug.Log(string.Concat(new object[]
					{
						"SpawnObject instance ID ",
						networkIdentity.netId,
						" asset ID ",
						networkIdentity.assetId
					}));
				}
				networkIdentity.RebuildObservers(true);
			}
		}

		internal void SendSpawnMessage(NetworkIdentity uv, NetworkConnection conn)
		{
			if (!uv.serverOnly)
			{
				if (uv.sceneId.IsEmpty())
				{
					ObjectSpawnMessage objectSpawnMessage = new ObjectSpawnMessage();
					objectSpawnMessage.netId = uv.netId;
					objectSpawnMessage.assetId = uv.assetId;
					objectSpawnMessage.position = uv.transform.position;
					objectSpawnMessage.rotation = uv.transform.rotation;
					NetworkWriter networkWriter = new NetworkWriter();
					uv.UNetSerializeAllVars(networkWriter);
					if (networkWriter.Position > 0)
					{
						objectSpawnMessage.payload = networkWriter.ToArray();
					}
					if (conn != null)
					{
						conn.Send(3, objectSpawnMessage);
					}
					else
					{
						NetworkServer.SendToReady(uv.gameObject, 3, objectSpawnMessage);
					}
				}
				else
				{
					ObjectSpawnSceneMessage objectSpawnSceneMessage = new ObjectSpawnSceneMessage();
					objectSpawnSceneMessage.netId = uv.netId;
					objectSpawnSceneMessage.sceneId = uv.sceneId;
					objectSpawnSceneMessage.position = uv.transform.position;
					NetworkWriter networkWriter2 = new NetworkWriter();
					uv.UNetSerializeAllVars(networkWriter2);
					if (networkWriter2.Position > 0)
					{
						objectSpawnSceneMessage.payload = networkWriter2.ToArray();
					}
					if (conn != null)
					{
						conn.Send(10, objectSpawnSceneMessage);
					}
					else
					{
						NetworkServer.SendToReady(uv.gameObject, 3, objectSpawnSceneMessage);
					}
				}
			}
		}

		public static void DestroyPlayersForConnection(NetworkConnection conn)
		{
			if (conn.playerControllers.Count == 0)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("Empty player list given to NetworkServer.Destroy(), nothing to do.");
				}
			}
			else
			{
				if (conn.clientOwnedObjects != null)
				{
					HashSet<NetworkInstanceId> hashSet = new HashSet<NetworkInstanceId>(conn.clientOwnedObjects);
					foreach (NetworkInstanceId netId in hashSet)
					{
						GameObject gameObject = NetworkServer.FindLocalObject(netId);
						if (gameObject != null)
						{
							NetworkServer.DestroyObject(gameObject);
						}
					}
				}
				for (int i = 0; i < conn.playerControllers.Count; i++)
				{
					PlayerController playerController = conn.playerControllers[i];
					if (playerController.IsValid)
					{
						if (!(playerController.unetView == null))
						{
							NetworkServer.DestroyObject(playerController.unetView, true);
						}
						playerController.gameObject = null;
					}
				}
				conn.playerControllers.Clear();
			}
		}

		private static void UnSpawnObject(GameObject obj)
		{
			NetworkIdentity uv;
			if (obj == null)
			{
				if (LogFilter.logDev)
				{
					Debug.Log("NetworkServer UnspawnObject is null");
				}
			}
			else if (NetworkServer.GetNetworkIdentity(obj, out uv))
			{
				NetworkServer.UnSpawnObject(uv);
			}
		}

		private static void UnSpawnObject(NetworkIdentity uv)
		{
			NetworkServer.DestroyObject(uv, false);
		}

		private static void DestroyObject(GameObject obj)
		{
			NetworkIdentity uv;
			if (obj == null)
			{
				if (LogFilter.logDev)
				{
					Debug.Log("NetworkServer DestroyObject is null");
				}
			}
			else if (NetworkServer.GetNetworkIdentity(obj, out uv))
			{
				NetworkServer.DestroyObject(uv, true);
			}
		}

		private static void DestroyObject(NetworkIdentity uv, bool destroyServerObject)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("DestroyObject instance:" + uv.netId);
			}
			if (NetworkServer.objects.ContainsKey(uv.netId))
			{
				NetworkServer.objects.Remove(uv.netId);
			}
			if (uv.clientAuthorityOwner != null)
			{
				uv.clientAuthorityOwner.RemoveOwnedObject(uv);
			}
			ObjectDestroyMessage objectDestroyMessage = new ObjectDestroyMessage();
			objectDestroyMessage.netId = uv.netId;
			NetworkServer.SendToObservers(uv.gameObject, 1, objectDestroyMessage);
			uv.ClearObservers();
			if (NetworkClient.active && instance.m_LocalClientActive)
			{
				uv.OnNetworkDestroy();
				ClientScene.SetLocalObject(objectDestroyMessage.netId, null);
			}
			if (destroyServerObject)
			{
				Object.Destroy(uv.gameObject);
			}
			uv.MarkForReset();
		}

		public static void ClearLocalObjects()
		{
			NetworkServer.objects.Clear();
		}

		public static void Spawn(GameObject obj)
		{
			if (NetworkServer.VerifyCanSpawn(obj))
			{
				instance.SpawnObject(obj);
			}
		}

		private static bool CheckForPrefab(GameObject obj)
		{
			return false;
		}

		private static bool VerifyCanSpawn(GameObject obj)
		{
			bool result;
			if (NetworkServer.CheckForPrefab(obj))
			{
				Debug.LogErrorFormat("GameObject {0} is a prefab, it can't be spawned. This will cause errors in builds.", new object[]
				{
					obj.name
				});
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		public static bool SpawnWithClientAuthority(GameObject obj, GameObject player)
		{
			NetworkIdentity component = player.GetComponent<NetworkIdentity>();
			bool result;
			if (component == null)
			{
				Debug.LogError("SpawnWithClientAuthority player object has no NetworkIdentity");
				result = false;
			}
			else if (component.connectionToClient == null)
			{
				Debug.LogError("SpawnWithClientAuthority player object is not a player.");
				result = false;
			}
			else
			{
				result = NetworkServer.SpawnWithClientAuthority(obj, component.connectionToClient);
			}
			return result;
		}

		public static bool SpawnWithClientAuthority(GameObject obj, QSBNetworkConnection conn)
		{
			bool result;
			if (!conn.isReady)
			{
				Debug.LogError("SpawnWithClientAuthority NetworkConnection is not ready!");
				result = false;
			}
			else
			{
				NetworkServer.Spawn(obj);
				NetworkIdentity component = obj.GetComponent<NetworkIdentity>();
				result = (!(component == null) && component.isServer && component.AssignClientAuthority(conn));
			}
			return result;
		}

		public static bool SpawnWithClientAuthority(GameObject obj, NetworkHash128 assetId, NetworkConnection conn)
		{
			NetworkServer.Spawn(obj, assetId);
			NetworkIdentity component = obj.GetComponent<NetworkIdentity>();
			return !(component == null) && component.isServer && component.AssignClientAuthority(conn);
		}

		public static void Spawn(GameObject obj, NetworkHash128 assetId)
		{
			if (NetworkServer.VerifyCanSpawn(obj))
			{
				NetworkIdentity networkIdentity;
				if (NetworkServer.GetNetworkIdentity(obj, out networkIdentity))
				{
					networkIdentity.SetDynamicAssetId(assetId);
				}
				instance.SpawnObject(obj);
			}
		}

		public static void Destroy(GameObject obj)
		{
			NetworkServer.DestroyObject(obj);
		}

		public static void UnSpawn(GameObject obj)
		{
			NetworkServer.UnSpawnObject(obj);
		}

		internal bool InvokeBytes(ULocalConnectionToServer conn, byte[] buffer, int numBytes, int channelId)
		{
			NetworkReader networkReader = new NetworkReader(buffer);
			networkReader.ReadInt16();
			short num = networkReader.ReadInt16();
			bool result;
			if (NetworkServer.handlers.ContainsKey(num) && m_LocalConnection != null)
			{
				m_LocalConnection.InvokeHandler(num, networkReader, channelId);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		internal bool InvokeHandlerOnServer(ULocalConnectionToServer conn, short msgType, MessageBase msg, int channelId)
		{
			bool result;
			if (NetworkServer.handlers.ContainsKey(msgType) && m_LocalConnection != null)
			{
				NetworkWriter writer = new NetworkWriter();
				msg.Serialize(writer);
				NetworkReader reader = new NetworkReader(writer);
				m_LocalConnection.InvokeHandler(msgType, reader, channelId);
				result = true;
			}
			else
			{
				if (LogFilter.logError)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"Local invoke: Failed to find local connection to invoke handler on [connectionId=",
						conn.connectionId,
						"] for MsgId:",
						msgType
					}));
				}
				result = false;
			}
			return result;
		}

		public static GameObject FindLocalObject(NetworkInstanceId netId)
		{
			return instance.m_NetworkScene.FindLocalObject(netId);
		}

		public static Dictionary<short, NetworkConnection.PacketStat> GetConnectionStats()
		{
			Dictionary<short, NetworkConnection.PacketStat> dictionary = new Dictionary<short, NetworkConnection.PacketStat>();
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					foreach (short key in networkConnection.packetStats.Keys)
					{
						if (dictionary.ContainsKey(key))
						{
							NetworkConnection.PacketStat packetStat = dictionary[key];
							packetStat.count += networkConnection.packetStats[key].count;
							packetStat.bytes += networkConnection.packetStats[key].bytes;
							dictionary[key] = packetStat;
						}
						else
						{
							dictionary[key] = new NetworkConnection.PacketStat(networkConnection.packetStats[key]);
						}
					}
				}
			}
			return dictionary;
		}

		public static void ResetConnectionStats()
		{
			for (int i = 0; i < NetworkServer.connections.Count; i++)
			{
				NetworkConnection networkConnection = NetworkServer.connections[i];
				if (networkConnection != null)
				{
					networkConnection.ResetStats();
				}
			}
		}

		public static bool AddExternalConnection(NetworkConnection conn)
		{
			return instance.AddExternalConnectionInternal(conn);
		}

		private bool AddExternalConnectionInternal(NetworkConnection conn)
		{
			bool result;
			if (conn.connectionId < 0)
			{
				result = false;
			}
			else if (conn.connectionId < NetworkServer.connections.Count && NetworkServer.connections[conn.connectionId] != null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("AddExternalConnection failed, already connection for id:" + conn.connectionId);
				}
				result = false;
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("AddExternalConnection external connection " + conn.connectionId);
				}
				m_SimpleServerSimple.SetConnectionAtIndex(conn);
				m_ExternalConnections.Add(conn.connectionId);
				conn.InvokeHandlerNoData(32);
				result = true;
			}
			return result;
		}

		public static void RemoveExternalConnection(int connectionId)
		{
			instance.RemoveExternalConnectionInternal(connectionId);
		}

		private bool RemoveExternalConnectionInternal(int connectionId)
		{
			bool result;
			if (!m_ExternalConnections.Contains(connectionId))
			{
				if (LogFilter.logError)
				{
					Debug.LogError("RemoveExternalConnection failed, no connection for id:" + connectionId);
				}
				result = false;
			}
			else
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("RemoveExternalConnection external connection " + connectionId);
				}
				NetworkConnection networkConnection = m_SimpleServerSimple.FindConnection(connectionId);
				if (networkConnection != null)
				{
					networkConnection.RemoveObservers();
				}
				m_SimpleServerSimple.RemoveConnectionAtIndex(connectionId);
				result = true;
			}
			return result;
		}

		private static bool ValidateSceneObject(NetworkIdentity netId)
		{
			return netId.gameObject.hideFlags != HideFlags.NotEditable && netId.gameObject.hideFlags != HideFlags.HideAndDontSave && !netId.sceneId.IsEmpty();
		}

		public static bool SpawnObjects()
		{
			bool result;
			if (!NetworkServer.active)
			{
				result = true;
			}
			else
			{
				foreach (NetworkIdentity networkIdentity in Resources.FindObjectsOfTypeAll<NetworkIdentity>())
				{
					if (NetworkServer.ValidateSceneObject(networkIdentity))
					{
						if (LogFilter.logDebug)
						{
							Debug.Log(string.Concat(new object[]
							{
								"SpawnObjects sceneId:",
								networkIdentity.sceneId,
								" name:",
								networkIdentity.gameObject.name
							}));
						}
						networkIdentity.Reset();
						networkIdentity.gameObject.SetActive(true);
					}
				}
				NetworkIdentity[] array;
				foreach (NetworkIdentity networkIdentity2 in array)
				{
					if (NetworkServer.ValidateSceneObject(networkIdentity2))
					{
						NetworkServer.Spawn(networkIdentity2.gameObject);
						networkIdentity2.ForceAuthority(true);
					}
				}
				result = true;
			}
			return result;
		}

		private static void SendCrc(NetworkConnection targetConnection)
		{
			if (NetworkCRC.singleton != null)
			{
				if (NetworkCRC.scriptCRCCheck)
				{
					CRCMessage crcmessage = new CRCMessage();
					List<CRCMessageEntry> list = new List<CRCMessageEntry>();
					foreach (string text in NetworkCRC.singleton.scripts.Keys)
					{
						list.Add(new CRCMessageEntry
						{
							name = text,
							channel = (byte)NetworkCRC.singleton.scripts[text]
						});
					}
					crcmessage.scripts = list.ToArray();
					targetConnection.Send(14, crcmessage);
				}
			}
		}

		[Obsolete("moved to NetworkMigrationManager")]
		public void SendNetworkInfo(NetworkConnection targetConnection)
		{
		}

		private static bool s_Active;

		private static volatile QSBNetworkServer s_Instance;

		private static object s_Sync = new Object();

		private static bool m_DontListen;

		private bool m_LocalClientActive;

		private List<QSBNetworkConnection> m_LocalConnectionsFakeList = new List<NetworkConnection>();

		private ULocalConnectionToClient m_LocalConnection = null;

		private NetworkScene m_NetworkScene;

		private HashSet<int> m_ExternalConnections;

		private ServerSimpleWrapper m_SimpleServerSimple;

		private float m_MaxDelay = 0.1f;

		private HashSet<NetworkInstanceId> m_RemoveList;

		private int m_RemoveListCount;

		private const int k_RemoveListInterval = 100;

		internal static ushort maxPacketSize;

		private static RemovePlayerMessage s_RemovePlayerMessage = new RemovePlayerMessage();

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache0;

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache1;

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache2;

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache3;

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache4;

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache5;

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache6;

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache7;

		[CompilerGenerated]
		private static NetworkMessageDelegate<> f__mg$cache8;

		private class ServerSimpleWrapper : NetworkServerSimple
		{
			public ServerSimpleWrapper(QSBNetworkServer server)
			{
				m_Server = server;
			}

			public override void OnConnectError(int connectionId, byte error)
			{
				m_Server.GenerateConnectError((int)error);
			}

			public override void OnDataError(QSBNetworkConnection conn, byte error)
			{
				m_Server.GenerateDataError(conn, (int)error);
			}

			public override void OnDisconnectError(QSBNetworkConnection conn, byte error)
			{
				m_Server.GenerateDisconnectError(conn, (int)error);
			}

			public override void OnConnected(QSBNetworkConnection conn)
			{
				m_Server.OnConnected(conn);
			}

			public override void OnDisconnected(QSBNetworkConnection conn)
			{
				m_Server.OnDisconnected(conn);
			}

			public override void OnData(QSBNetworkConnection conn, int receivedSize, int channelId)
			{
				m_Server.OnData(conn, receivedSize, channelId);
			}

			private QSBNetworkServer m_Server;
		}
	}
}
