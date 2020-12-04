using OWML.Common;
using QSB.Animation;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

namespace QSB.QuantumUNET
{
	public class QSBNetworkServer
	{
		private QSBNetworkServer()
		{
			NetworkTransport.Init();
			m_RemoveList = new HashSet<NetworkInstanceId>();
			m_ExternalConnections = new HashSet<int>();
			m_NetworkScene = new QSBNetworkScene();
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

		public static Dictionary<short, QSBNetworkMessageDelegate> handlers
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
					var obj = s_Sync;
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
				return s_Active;
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

		public static void SetNetworkConnectionClass<T>() where T : QSBNetworkConnection
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
				instance.InternalListenRelay(matchInfo.address, matchInfo.port, matchInfo.networkId, QSBUtility.GetSourceID(), matchInfo.nodeId);
				result = true;
			}
			return result;
		}

		internal void RegisterMessageHandlers()
		{
			m_SimpleServerSimple.RegisterHandlerSafe((short)35, new QSBNetworkMessageDelegate(OnClientReadyMessage));
			m_SimpleServerSimple.RegisterHandlerSafe((short)5, new QSBNetworkMessageDelegate(OnCommandMessage));
			m_SimpleServerSimple.RegisterHandlerSafe(6, new QSBNetworkMessageDelegate(QSBNetworkTransform.HandleTransform));
			//m_SimpleServerSimple.RegisterHandlerSafe((short)16, new QSBNetworkMessageDelegate(NetworkTransformChild.HandleChildTransform));
			m_SimpleServerSimple.RegisterHandlerSafe((short)38, new QSBNetworkMessageDelegate(OnRemovePlayerMessage));
			m_SimpleServerSimple.RegisterHandlerSafe((short)40, new QSBNetworkMessageDelegate(QSBNetworkAnimator.OnAnimationServerMessage));
			m_SimpleServerSimple.RegisterHandlerSafe((short)41, new QSBNetworkMessageDelegate(QSBNetworkAnimator.OnAnimationParametersServerMessage));
			m_SimpleServerSimple.RegisterHandlerSafe((short)42, new QSBNetworkMessageDelegate(QSBNetworkAnimator.OnAnimationTriggerServerMessage));
			maxPacketSize = hostTopology.DefaultConfig.PacketSize;
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
			maxPacketSize = hostTopology.DefaultConfig.PacketSize;
			s_Active = true;
			RegisterMessageHandlers();
			return true;
		}

		public static QSBNetworkClient BecomeHost(QSBNetworkClient oldClient, int port, MatchInfo matchInfo, int oldConnectionId, QSBPeerInfoMessage[] peers)
		{
			return instance.BecomeHostInternal(oldClient, port, matchInfo, oldConnectionId, peers);
		}

		internal QSBNetworkClient BecomeHostInternal(QSBNetworkClient oldClient, int port, MatchInfo matchInfo, int oldConnectionId, QSBPeerInfoMessage[] peers)
		{
			QSBNetworkClient result;
			if (s_Active)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("BecomeHost already a server.");
				}
				result = null;
			}
			else if (!QSBNetworkClient.active)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("BecomeHost NetworkClient not active.");
				}
				result = null;
			}
			else
			{
				Configure(hostTopology);
				if (matchInfo == null)
				{
					Debug.Log("BecomeHost Listen on " + port);
					if (!Listen(port))
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
					Debug.Log("BecomeHost match:" + matchInfo.networkId);
					ListenRelay(matchInfo.address, matchInfo.port, matchInfo.networkId, QSBUtility.GetSourceID(), matchInfo.nodeId);
				}
				foreach (var networkIdentity in QSBClientScene.Objects.Values)
				{
					if (!(networkIdentity == null) && !(networkIdentity.gameObject == null))
					{
						QSBNetworkIdentity.AddNetworkId(networkIdentity.NetId.Value);
						m_NetworkScene.SetLocalObject(networkIdentity.NetId, networkIdentity.gameObject, false, false);
						networkIdentity.OnStartServer(true);
					}
				}
				Debug.Log("NetworkServer BecomeHost done. oldConnectionId:" + oldConnectionId);
				RegisterMessageHandlers();
				if (!QSBNetworkClient.RemoveClient(oldClient))
				{
					if (LogFilter.logError)
					{
						Debug.LogError("BecomeHost failed to remove client");
					}
				}
				Debug.Log("BecomeHost localClient ready");
				var networkClient = QSBClientScene.ReconnectLocalServer();
				QSBClientScene.Ready(networkClient.connection);
				QSBClientScene.SetReconnectId(oldConnectionId, peers);
				QSBClientScene.AddPlayer(QSBClientScene.readyConnection, 0);
				result = networkClient;
			}
			return result;
		}

		private void InternalSetMaxDelay(float seconds)
		{
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
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
				m_LocalConnection = new QSBULocalConnectionToClient(localClient);
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
			for (var i = 0; i < m_LocalConnectionsFakeList.Count; i++)
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
			Debug.Log(string.Concat(new object[]
			{
				"SetLocalObjectOnServer ",
				netId,
				" ",
				obj
			}));
			m_NetworkScene.SetLocalObject(netId, obj, false, true);
		}

		internal void ActivateLocalClientScene()
		{
			if (!m_LocalClientActive)
			{
				m_LocalClientActive = true;
				foreach (var networkIdentity in objects.Values)
				{
					if (!networkIdentity.IsClient)
					{
						Debug.Log(string.Concat(new object[]
						{
							"ActivateClientScene ",
							networkIdentity.NetId,
							" ",
							networkIdentity.gameObject
						}));
						QSBClientScene.SetLocalObject(networkIdentity.NetId, networkIdentity.gameObject);
						networkIdentity.OnStartClient();
					}
				}
			}
		}

		public static bool SendToAll(short msgType, QSBMessageBase msg)
		{
			Debug.Log("Server.SendToAll msgType:" + msgType);
			var flag = true;
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
				if (networkConnection != null)
				{
					flag &= networkConnection.Send(msgType, msg);
				}
			}
			return flag;
		}

		private static bool SendToObservers(GameObject contextObj, short msgType, QSBMessageBase msg)
		{
			Debug.Log("Server.SendToObservers id:" + msgType);
			var flag = true;
			var component = contextObj.GetComponent<QSBNetworkIdentity>();
			bool result;
			if (component == null || component.Observers == null)
			{
				result = false;
			}
			else
			{
				var count = component.Observers.Count;
				for (var i = 0; i < count; i++)
				{
					var networkConnection = component.Observers[i];
					flag &= networkConnection.Send(msgType, msg);
				}
				result = flag;
			}
			return result;
		}

		public static bool SendToReady(GameObject contextObj, short msgType, QSBMessageBase msg)
		{
			Debug.Log("Server.SendToReady id:" + msgType);
			bool result;
			if (contextObj == null)
			{
				for (var i = 0; i < connections.Count; i++)
				{
					var networkConnection = connections[i];
					if (networkConnection != null && networkConnection.isReady)
					{
						networkConnection.Send(msgType, msg);
					}
				}
				result = true;
			}
			else
			{
				var flag = true;
				var component = contextObj.GetComponent<QSBNetworkIdentity>();
				if (component == null || component.Observers == null)
				{
					result = false;
				}
				else
				{
					var count = component.Observers.Count;
					for (var j = 0; j < count; j++)
					{
						var networkConnection2 = component.Observers[j];
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

		public static void SendWriterToReady(GameObject contextObj, QSBNetworkWriter writer, int channelId)
		{
			DebugLog.DebugWrite("send writer to ready");
			var arraySegment = (ArraySegment<byte>)writer.GetType().GetMethod("AsArraySegment").Invoke(writer, null);
			if (arraySegment.Count > 32767)
			{
				throw new UnityException("NetworkWriter used buffer is too big!");
			}
			DebugLog.DebugWrite("pre send bytes");
			SendBytesToReady(contextObj, arraySegment.Array, arraySegment.Count, channelId);
		}

		public static void SendBytesToReady(GameObject contextObj, byte[] buffer, int numBytes, int channelId)
		{
			DebugLog.DebugWrite("send bytes to ready");
			if (contextObj == null)
			{
				var flag = true;
				for (var i = 0; i < connections.Count; i++)
				{
					var networkConnection = connections[i];
					if (networkConnection != null && networkConnection.isReady)
					{
						DebugLog.DebugWrite($"sending bytes to connection {networkConnection.connectionId}");
						if (!networkConnection.SendBytes(buffer, numBytes, channelId))
						{
							flag = false;
						}
					}
					else
					{
						DebugLog.DebugWrite($"- Connection {networkConnection.connectionId} is not ready!");
					}
				}
				if (!flag)
				{
					DebugLog.DebugWrite("SendBytesToReady failed");
				}
			}
			else
			{
				var component = contextObj.GetComponent<QSBNetworkIdentity>();
				try
				{
					var flag2 = true;
					var count = component.Observers.Count;
					for (var j = 0; j < count; j++)
					{
						var networkConnection2 = component.Observers[j];
						if (networkConnection2.isReady)
						{
							DebugLog.DebugWrite($"sending bytes to connection {networkConnection2.connectionId}");
							if (!networkConnection2.SendBytes(buffer, numBytes, channelId))
							{
								flag2 = false;
							}
						}
						else
						{
							DebugLog.DebugWrite($"- Connection {networkConnection2.connectionId} is not ready!");
						}
					}
					if (!flag2)
					{
						DebugLog.DebugWrite("SendBytesToReady failed for " + contextObj);
					}
				}
				catch (NullReferenceException)
				{
					DebugLog.DebugWrite("SendBytesToReady object " + contextObj + " has not been spawned");
				}
			}
		}

		public static void SendBytesToPlayer(GameObject player, byte[] buffer, int numBytes, int channelId)
		{
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
				if (networkConnection != null)
				{
					for (var j = 0; j < networkConnection.PlayerControllers.Count; j++)
					{
						if (networkConnection.PlayerControllers[j].IsValid && networkConnection.PlayerControllers[j].Gameobject == player)
						{
							networkConnection.SendBytes(buffer, numBytes, channelId);
							break;
						}
					}
				}
			}
		}

		public static bool SendUnreliableToAll(short msgType, QSBMessageBase msg)
		{
			Debug.Log("Server.SendUnreliableToAll msgType:" + msgType);
			var flag = true;
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
				if (networkConnection != null)
				{
					flag &= networkConnection.SendUnreliable(msgType, msg);
				}
			}
			return flag;
		}

		public static bool SendUnreliableToReady(GameObject contextObj, short msgType, QSBMessageBase msg)
		{
			Debug.Log("Server.SendUnreliableToReady id:" + msgType);
			bool result;
			if (contextObj == null)
			{
				for (var i = 0; i < connections.Count; i++)
				{
					var networkConnection = connections[i];
					if (networkConnection != null && networkConnection.isReady)
					{
						networkConnection.SendUnreliable(msgType, msg);
					}
				}
				result = true;
			}
			else
			{
				var flag = true;
				var component = contextObj.GetComponent<QSBNetworkIdentity>();
				var count = component.Observers.Count;
				for (var j = 0; j < count; j++)
				{
					var networkConnection2 = component.Observers[j];
					if (networkConnection2.isReady)
					{
						flag &= networkConnection2.SendUnreliable(msgType, msg);
					}
				}
				result = flag;
			}
			return result;
		}

		public static bool SendByChannelToAll(short msgType, QSBMessageBase msg, int channelId)
		{
			Debug.Log("Server.SendByChannelToAll id:" + msgType);
			var flag = true;
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
				if (networkConnection != null)
				{
					flag &= networkConnection.SendByChannel(msgType, msg, channelId);
				}
			}
			return flag;
		}

		public static bool SendByChannelToReady(GameObject contextObj, short msgType, QSBMessageBase msg, int channelId)
		{
			Debug.Log("Server.SendByChannelToReady msgType:" + msgType);
			bool result;
			if (contextObj == null)
			{
				for (var i = 0; i < connections.Count; i++)
				{
					var networkConnection = connections[i];
					if (networkConnection != null && networkConnection.isReady)
					{
						networkConnection.SendByChannel(msgType, msg, channelId);
					}
				}
				result = true;
			}
			else
			{
				var flag = true;
				var component = contextObj.GetComponent<QSBNetworkIdentity>();
				var count = component.Observers.Count;
				for (var j = 0; j < count; j++)
				{
					var networkConnection2 = component.Observers[j];
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
			if (s_Instance != null)
			{
				s_Instance.InternalUpdate();
			}
		}

		private void UpdateServerObjects()
		{
			foreach (var networkIdentity in objects.Values)
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
			foreach (var networkInstanceId in objects.Keys)
			{
				var networkIdentity = objects[networkInstanceId];
				if (networkIdentity == null || networkIdentity.gameObject == null)
				{
					m_RemoveList.Add(networkInstanceId);
				}
			}
			if (m_RemoveList.Count > 0)
			{
				foreach (var key in m_RemoveList)
				{
					objects.Remove(key);
				}
				m_RemoveList.Clear();
			}
		}

		internal void InternalUpdate()
		{
			m_SimpleServerSimple.Update();
			if (m_DontListen)
			{
				m_SimpleServerSimple.UpdateConnections();
			}
			UpdateServerObjects();
		}

		private void OnConnected(QSBNetworkConnection conn)
		{
			Debug.Log("Server accepted client:" + conn.connectionId);
			conn.SetMaxDelay(m_MaxDelay);
			conn.InvokeHandlerNoData(32);
			SendCrc(conn);
		}

		private void OnDisconnected(QSBNetworkConnection conn)
		{
			conn.InvokeHandlerNoData(33);
			for (var i = 0; i < conn.PlayerControllers.Count; i++)
			{
				if (conn.PlayerControllers[i].Gameobject != null)
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

		private void OnData(QSBNetworkConnection conn, int receivedSize, int channelId)
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

		private void GenerateDataError(QSBNetworkConnection conn, int error)
		{
			if (LogFilter.logError)
			{
				Debug.LogError("UNet Server Data Error: " + (NetworkError)error);
			}
			GenerateError(conn, error);
		}

		private void GenerateDisconnectError(QSBNetworkConnection conn, int error)
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

		private void GenerateError(QSBNetworkConnection conn, int error)
		{
			if (handlers.ContainsKey(34))
			{
				var errorMessage = new QSBErrorMessage();
				errorMessage.errorCode = error;
				var writer = new QSBNetworkWriter();
				errorMessage.Serialize(writer);
				var reader = new QSBNetworkReader(writer);
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
			QSBNetworkScene.ClearSpawners();
		}

		public static void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
		{
			numMsgs = 0;
			numBufferedMsgs = 0;
			numBytes = 0;
			lastBufferedPerSecond = 0;
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
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
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
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

		public static void SendToClientOfPlayer(GameObject player, short msgType, QSBMessageBase msg)
		{
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
				if (networkConnection != null)
				{
					for (var j = 0; j < networkConnection.PlayerControllers.Count; j++)
					{
						if (networkConnection.PlayerControllers[j].IsValid && networkConnection.PlayerControllers[j].Gameobject == player)
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

		public static void SendToClient(int connectionId, short msgType, QSBMessageBase msg)
		{
			if (connectionId < connections.Count)
			{
				var networkConnection = connections[connectionId];
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

		public static bool ReplacePlayerForConnection(QSBNetworkConnection conn, GameObject player, short playerControllerId, NetworkHash128 assetId)
		{
			QSBNetworkIdentity networkIdentity;
			if (GetNetworkIdentity(player, out networkIdentity))
			{
				networkIdentity.SetDynamicAssetId(assetId);
			}
			return instance.InternalReplacePlayerForConnection(conn, player, playerControllerId);
		}

		public static bool ReplacePlayerForConnection(QSBNetworkConnection conn, GameObject player, short playerControllerId)
		{
			return instance.InternalReplacePlayerForConnection(conn, player, playerControllerId);
		}

		public static bool AddPlayerForConnection(QSBNetworkConnection conn, GameObject player, short playerControllerId, NetworkHash128 assetId)
		{
			QSBNetworkIdentity networkIdentity;
			if (GetNetworkIdentity(player, out networkIdentity))
			{
				networkIdentity.SetDynamicAssetId(assetId);
			}
			return instance.InternalAddPlayerForConnection(conn, player, playerControllerId);
		}

		public static bool AddPlayerForConnection(QSBNetworkConnection conn, GameObject player, short playerControllerId)
		{
			return instance.InternalAddPlayerForConnection(conn, player, playerControllerId);
		}

		internal bool InternalAddPlayerForConnection(QSBNetworkConnection conn, GameObject playerGameObject, short playerControllerId)
		{
			QSBNetworkIdentity networkIdentity;
			bool result;
			if (!GetNetworkIdentity(playerGameObject, out networkIdentity))
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
				if (!CheckPlayerControllerIdForConnection(conn, playerControllerId))
				{
					result = false;
				}
				else
				{
					QSBPlayerController playerController = null;
					GameObject x = null;
					if (conn.GetPlayerController(playerControllerId, out playerController))
					{
						x = playerController.Gameobject;
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
						var playerController2 = new QSBPlayerController(playerGameObject, playerControllerId);
						conn.SetPlayerController(playerController2);
						networkIdentity.SetConnectionToClient(conn, playerController2.PlayerControllerId);
						SetClientReady(conn);
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
									playerGameObject.GetComponent<QSBNetworkIdentity>().NetId,
									" asset ID ",
									playerGameObject.GetComponent<QSBNetworkIdentity>().AssetId
								}));
							}
							FinishPlayerForConnection(conn, networkIdentity, playerGameObject);
							if (networkIdentity.LocalPlayerAuthority)
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

		private static bool CheckPlayerControllerIdForConnection(QSBNetworkConnection conn, short playerControllerId)
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

		private bool SetupLocalPlayerForConnection(QSBNetworkConnection conn, QSBNetworkIdentity uv, QSBPlayerController newPlayerController)
		{
			Debug.Log("NetworkServer SetupLocalPlayerForConnection netID:" + uv.NetId);
			var ulocalConnectionToClient = conn as QSBULocalConnectionToClient;
			bool result;
			if (ulocalConnectionToClient != null)
			{
				Debug.Log("NetworkServer AddPlayer handling ULocalConnectionToClient");
				if (uv.NetId.IsEmpty())
				{
					uv.OnStartServer(true);
				}
				uv.RebuildObservers(true);
				SendSpawnMessage(uv, null);
				ulocalConnectionToClient.localClient.AddLocalPlayer(newPlayerController);
				uv.SetClientOwner(conn);
				uv.ForceAuthority(true);
				uv.SetLocalPlayer(newPlayerController.PlayerControllerId);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		private static void FinishPlayerForConnection(QSBNetworkConnection conn, QSBNetworkIdentity uv, GameObject playerGameObject)
		{
			if (uv.NetId.IsEmpty())
			{
				Spawn(playerGameObject);
			}
			conn.Send(4, new QSBOwnerMessage
			{
				NetId = uv.NetId,
				PlayerControllerId = uv.PlayerControllerId
			});
		}

		internal bool InternalReplacePlayerForConnection(QSBNetworkConnection conn, GameObject playerGameObject, short playerControllerId)
		{
			QSBNetworkIdentity networkIdentity;
			bool result;
			if (!GetNetworkIdentity(playerGameObject, out networkIdentity))
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ReplacePlayer: playerGameObject has no NetworkIdentity. Please add a NetworkIdentity to " + playerGameObject);
				}
				result = false;
			}
			else if (!CheckPlayerControllerIdForConnection(conn, playerControllerId))
			{
				result = false;
			}
			else
			{
				Debug.Log("NetworkServer ReplacePlayer");
				QSBPlayerController playerController;
				if (conn.GetPlayerController(playerControllerId, out playerController))
				{
					playerController.UnetView.SetNotLocalPlayer();
					playerController.UnetView.ClearClientOwner();
				}
				var playerController2 = new QSBPlayerController(playerGameObject, playerControllerId);
				conn.SetPlayerController(playerController2);
				networkIdentity.SetConnectionToClient(conn, playerController2.PlayerControllerId);
				Debug.Log("NetworkServer ReplacePlayer setup local");
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
					FinishPlayerForConnection(conn, networkIdentity, playerGameObject);
					if (networkIdentity.LocalPlayerAuthority)
					{
						networkIdentity.SetClientOwner(conn);
					}
					result = true;
				}
			}
			return result;
		}

		private static bool GetNetworkIdentity(GameObject go, out QSBNetworkIdentity view)
		{
			view = go.GetComponent<QSBNetworkIdentity>();
			bool result;
			if (view == null)
			{
				Debug.LogError("UNET failure. GameObject doesn't have NetworkIdentity.");
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		public static void SetClientReady(QSBNetworkConnection conn)
		{
			instance.SetClientReadyInternal(conn);
		}

		internal void SetClientReadyInternal(QSBNetworkConnection conn)
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
				if (conn.PlayerControllers.Count == 0)
				{
					if (LogFilter.logDebug)
					{
						Debug.LogWarning("Ready with no player object");
					}
				}
				conn.isReady = true;
				var ulocalConnectionToClient = conn as QSBULocalConnectionToClient;
				if (ulocalConnectionToClient != null)
				{
					Debug.Log("NetworkServer Ready handling ULocalConnectionToClient");
					foreach (var networkIdentity in objects.Values)
					{
						if (networkIdentity != null && networkIdentity.gameObject != null)
						{
							bool flag = networkIdentity.OnCheckObserver(conn);
							if (flag)
							{
								networkIdentity.AddObserver(conn);
							}
							if (!networkIdentity.IsClient)
							{
								Debug.Log("LocalClient.SetSpawnObject calling OnStartClient");
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
							objects.Count,
							" objects for conn ",
							conn.connectionId
						}));
					}
					var objectSpawnFinishedMessage = new QSBObjectSpawnFinishedMessage();
					objectSpawnFinishedMessage.State = 0U;
					conn.Send(12, objectSpawnFinishedMessage);
					foreach (var networkIdentity2 in objects.Values)
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
									networkIdentity2.NetId
								}));
							}
							bool flag2 = networkIdentity2.OnCheckObserver(conn);
							if (flag2)
							{
								networkIdentity2.AddObserver(conn);
							}
						}
					}
					objectSpawnFinishedMessage.State = 1U;
					conn.Send(12, objectSpawnFinishedMessage);
				}
			}
		}

		internal static void ShowForConnection(QSBNetworkIdentity uv, QSBNetworkConnection conn)
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
				NetId = uv.NetId
			});
		}

		public static void SetAllClientsNotReady()
		{
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
				if (networkConnection != null)
				{
					SetClientNotReady(networkConnection);
				}
			}
		}

		public static void SetClientNotReady(QSBNetworkConnection conn)
		{
			instance.InternalSetClientNotReady(conn);
		}

		internal void InternalSetClientNotReady(QSBNetworkConnection conn)
		{
			if (conn.isReady)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("PlayerNotReady " + conn);
				}
				conn.isReady = false;
				conn.RemoveObservers();
				var msg = new QSBNotReadyMessage();
				conn.Send(36, msg);
			}
		}

		private static void OnClientReadyMessage(QSBNetworkMessage netMsg)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("Default handler for ready message from " + netMsg.Connection);
			}
			SetClientReady(netMsg.Connection);
		}

		private static void OnRemovePlayerMessage(QSBNetworkMessage netMsg)
		{
			netMsg.ReadMessage<QSBRemovePlayerMessage>(s_RemovePlayerMessage);
			QSBPlayerController playerController = null;
			netMsg.Connection.GetPlayerController(s_RemovePlayerMessage.PlayerControllerId, out playerController);
			if (playerController != null)
			{
				netMsg.Connection.RemovePlayerController(s_RemovePlayerMessage.PlayerControllerId);
				Destroy(playerController.Gameobject);
			}
			else if (LogFilter.logError)
			{
				Debug.LogError("Received remove player message but could not find the player ID: " + s_RemovePlayerMessage.PlayerControllerId);
			}
		}

		private static void OnCommandMessage(QSBNetworkMessage netMsg)
		{
			var cmdHash = (int)netMsg.Reader.ReadPackedUInt32();
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var gameObject = FindLocalObject(networkInstanceId);
			if (gameObject == null)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("Instance not found when handling Command message [netId=" + networkInstanceId + "]");
				}
			}
			else
			{
				var component = gameObject.GetComponent<QSBNetworkIdentity>();
				if (component == null)
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("NetworkIdentity deleted when handling Command message [netId=" + networkInstanceId + "]");
					}
				}
				else
				{
					var flag = false;
					for (var i = 0; i < netMsg.Connection.PlayerControllers.Count; i++)
					{
						var playerController = netMsg.Connection.PlayerControllers[i];
						if (playerController.Gameobject != null && playerController.Gameobject.GetComponent<QSBNetworkIdentity>().NetId == component.NetId)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						if (component.ClientAuthorityOwner != netMsg.Connection)
						{
							if (LogFilter.logWarn)
							{
								Debug.LogWarning("Command for object without authority [netId=" + networkInstanceId + "]");
							}
							return;
						}
					}
					Debug.Log(string.Concat(new object[]
					{
						"OnCommandMessage for netId=",
						networkInstanceId,
						" conn=",
						netMsg.Connection
					}));
					component.HandleCommand(cmdHash, netMsg.Reader);
				}
			}
		}

		internal void SpawnObject(GameObject obj)
		{
			QSBNetworkIdentity networkIdentity;
			if (!active)
			{
				DebugLog.ToConsole("Error - SpawnObject for " + obj + ", NetworkServer is not active. Cannot spawn objects without an active server.", MessageType.Error);
			}
			else if (!GetNetworkIdentity(obj, out networkIdentity))
			{
				Debug.LogError(string.Concat(new object[]
				{
					"SpawnObject ",
					obj,
					" has no QSBNetworkIdentity. Please add a NetworkIdentity to ",
					obj
				}));
			}
			else
			{
				networkIdentity.Reset();
				networkIdentity.OnStartServer(false);
				DebugLog.DebugWrite(string.Concat(new object[]
				{
					"SpawnObject instance ID ",
					networkIdentity.NetId,
					" asset ID ",
					networkIdentity.AssetId
				}));
				networkIdentity.RebuildObservers(true);
			}
		}

		internal void SendSpawnMessage(QSBNetworkIdentity uv, QSBNetworkConnection conn)
		{
			if (!uv.ServerOnly)
			{
				if (uv.SceneId.IsEmpty())
				{
					var objectSpawnMessage = new QSBObjectSpawnMessage();
					objectSpawnMessage.NetId = uv.NetId;
					objectSpawnMessage.assetId = uv.AssetId;
					objectSpawnMessage.Position = uv.transform.position;
					objectSpawnMessage.Rotation = uv.transform.rotation;
					var networkWriter = new QSBNetworkWriter();
					uv.UNetSerializeAllVars(networkWriter);
					if (networkWriter.Position > 0)
					{
						objectSpawnMessage.Payload = networkWriter.ToArray();
					}
					if (conn != null)
					{
						conn.Send(3, objectSpawnMessage);
					}
					else
					{
						SendToReady(uv.gameObject, 3, objectSpawnMessage);
					}
				}
				else
				{
					var objectSpawnSceneMessage = new QSBObjectSpawnSceneMessage();
					objectSpawnSceneMessage.NetId = uv.NetId;
					objectSpawnSceneMessage.SceneId = uv.SceneId;
					objectSpawnSceneMessage.Position = uv.transform.position;
					var networkWriter2 = new QSBNetworkWriter();
					uv.UNetSerializeAllVars(networkWriter2);
					if (networkWriter2.Position > 0)
					{
						objectSpawnSceneMessage.Payload = networkWriter2.ToArray();
					}
					if (conn != null)
					{
						conn.Send(10, objectSpawnSceneMessage);
					}
					else
					{
						SendToReady(uv.gameObject, 3, objectSpawnSceneMessage);
					}
				}
			}
		}

		public static void DestroyPlayersForConnection(QSBNetworkConnection conn)
		{
			if (conn.PlayerControllers.Count == 0)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("Empty player list given to NetworkServer.Destroy(), nothing to do.");
				}
			}
			else
			{
				if (conn.ClientOwnedObjects != null)
				{
					var hashSet = new HashSet<NetworkInstanceId>(conn.ClientOwnedObjects);
					foreach (var netId in hashSet)
					{
						var gameObject = FindLocalObject(netId);
						if (gameObject != null)
						{
							DestroyObject(gameObject);
						}
					}
				}
				for (var i = 0; i < conn.PlayerControllers.Count; i++)
				{
					var playerController = conn.PlayerControllers[i];
					if (playerController.IsValid)
					{
						if (!(playerController.UnetView == null))
						{
							DestroyObject(playerController.UnetView, true);
						}
						playerController.Gameobject = null;
					}
				}
				conn.PlayerControllers.Clear();
			}
		}

		private static void UnSpawnObject(GameObject obj)
		{
			QSBNetworkIdentity uv;
			if (obj == null)
			{
				Debug.Log("NetworkServer UnspawnObject is null");
			}
			else if (GetNetworkIdentity(obj, out uv))
			{
				UnSpawnObject(uv);
			}
		}

		private static void UnSpawnObject(QSBNetworkIdentity uv)
		{
			DestroyObject(uv, false);
		}

		private static void DestroyObject(GameObject obj)
		{
			QSBNetworkIdentity uv;
			if (obj == null)
			{
				Debug.Log("NetworkServer DestroyObject is null");
			}
			else if (GetNetworkIdentity(obj, out uv))
			{
				DestroyObject(uv, true);
			}
		}

		private static void DestroyObject(QSBNetworkIdentity uv, bool destroyServerObject)
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("DestroyObject instance:" + uv.NetId);
			}
			if (objects.ContainsKey(uv.NetId))
			{
				objects.Remove(uv.NetId);
			}
			if (uv.ClientAuthorityOwner != null)
			{
				uv.ClientAuthorityOwner.RemoveOwnedObject(uv);
			}
			var objectDestroyMessage = new QSBObjectDestroyMessage();
			objectDestroyMessage.NetId = uv.NetId;
			SendToObservers(uv.gameObject, 1, objectDestroyMessage);
			uv.ClearObservers();
			if (QSBNetworkClient.active && instance.m_LocalClientActive)
			{
				uv.OnNetworkDestroy();
				QSBClientScene.SetLocalObject(objectDestroyMessage.NetId, null);
			}
			if (destroyServerObject)
			{
				UnityEngine.Object.Destroy(uv.gameObject);
			}
			uv.MarkForReset();
		}

		public static void ClearLocalObjects()
		{
			objects.Clear();
		}

		public static void Spawn(GameObject obj)
		{
			if (VerifyCanSpawn(obj))
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
			if (CheckForPrefab(obj))
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
			var component = player.GetComponent<QSBNetworkIdentity>();
			bool result;
			if (component == null)
			{
				Debug.LogError("SpawnWithClientAuthority player object has no NetworkIdentity");
				result = false;
			}
			else if (component.ConnectionToClient == null)
			{
				Debug.LogError("SpawnWithClientAuthority player object is not a player.");
				result = false;
			}
			else
			{
				result = SpawnWithClientAuthority(obj, component.ConnectionToClient);
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
				Spawn(obj);
				var component = obj.GetComponent<QSBNetworkIdentity>();
				result = (!(component == null) && component.IsServer && component.AssignClientAuthority(conn));
			}
			return result;
		}

		public static bool SpawnWithClientAuthority(GameObject obj, NetworkHash128 assetId, QSBNetworkConnection conn)
		{
			Spawn(obj, assetId);
			var component = obj.GetComponent<QSBNetworkIdentity>();
			return !(component == null) && component.IsServer && component.AssignClientAuthority(conn);
		}

		public static void Spawn(GameObject obj, NetworkHash128 assetId)
		{
			if (VerifyCanSpawn(obj))
			{
				QSBNetworkIdentity networkIdentity;
				if (GetNetworkIdentity(obj, out networkIdentity))
				{
					networkIdentity.SetDynamicAssetId(assetId);
				}
				instance.SpawnObject(obj);
			}
		}

		public static void Destroy(GameObject obj)
		{
			DestroyObject(obj);
		}

		public static void UnSpawn(GameObject obj)
		{
			UnSpawnObject(obj);
		}

		internal bool InvokeBytes(QSBULocalConnectionToServer conn, byte[] buffer, int numBytes, int channelId)
		{
			var networkReader = new QSBNetworkReader(buffer);
			networkReader.ReadInt16();
			var num = networkReader.ReadInt16();
			bool result;
			if (handlers.ContainsKey(num) && m_LocalConnection != null)
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

		internal bool InvokeHandlerOnServer(QSBULocalConnectionToServer conn, short msgType, QSBMessageBase msg, int channelId)
		{
			bool result;
			if (handlers.ContainsKey(msgType) && m_LocalConnection != null)
			{
				var writer = new QSBNetworkWriter();
				msg.Serialize(writer);
				var reader = new QSBNetworkReader(writer);
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

		public static Dictionary<short, QSBNetworkConnection.PacketStat> GetConnectionStats()
		{
			var dictionary = new Dictionary<short, QSBNetworkConnection.PacketStat>();
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
				if (networkConnection != null)
				{
					foreach (short key in networkConnection.PacketStats.Keys)
					{
						if (dictionary.ContainsKey(key))
						{
							var packetStat = dictionary[key];
							packetStat.count += networkConnection.PacketStats[key].count;
							packetStat.bytes += networkConnection.PacketStats[key].bytes;
							dictionary[key] = packetStat;
						}
						else
						{
							dictionary[key] = new QSBNetworkConnection.PacketStat(networkConnection.PacketStats[key]);
						}
					}
				}
			}
			return dictionary;
		}

		public static void ResetConnectionStats()
		{
			for (var i = 0; i < connections.Count; i++)
			{
				var networkConnection = connections[i];
				if (networkConnection != null)
				{
					networkConnection.ResetStats();
				}
			}
		}

		public static bool AddExternalConnection(QSBNetworkConnection conn)
		{
			return instance.AddExternalConnectionInternal(conn);
		}

		private bool AddExternalConnectionInternal(QSBNetworkConnection conn)
		{
			bool result;
			if (conn.connectionId < 0)
			{
				result = false;
			}
			else if (conn.connectionId < connections.Count && connections[conn.connectionId] != null)
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
				var networkConnection = m_SimpleServerSimple.FindConnection(connectionId);
				if (networkConnection != null)
				{
					networkConnection.RemoveObservers();
				}
				m_SimpleServerSimple.RemoveConnectionAtIndex(connectionId);
				result = true;
			}
			return result;
		}

		private static bool ValidateSceneObject(QSBNetworkIdentity netId)
		{
			return netId.gameObject.hideFlags != HideFlags.NotEditable && netId.gameObject.hideFlags != HideFlags.HideAndDontSave && !netId.SceneId.IsEmpty();
		}

		public static bool SpawnObjects()
		{
			bool result;
			if (!active)
			{
				result = true;
			}
			else
			{
				var objectsOfTypeAll = Resources.FindObjectsOfTypeAll<QSBNetworkIdentity>();
				foreach (var networkIdentity in objectsOfTypeAll)
				{
					if (ValidateSceneObject(networkIdentity))
					{
						if (LogFilter.logDebug)
						{
							Debug.Log(string.Concat(new object[]
							{
								"SpawnObjects sceneId:",
								networkIdentity.SceneId,
								" name:",
								networkIdentity.gameObject.name
							}));
						}
						networkIdentity.Reset();
						networkIdentity.gameObject.SetActive(true);
					}
				}
				foreach (var networkIdentity2 in objectsOfTypeAll)
				{
					if (ValidateSceneObject(networkIdentity2))
					{
						Spawn(networkIdentity2.gameObject);
						networkIdentity2.ForceAuthority(true);
					}
				}
				result = true;
			}
			return result;
		}

		private static void SendCrc(QSBNetworkConnection targetConnection)
		{
			if (QSBNetworkCRC.singleton != null)
			{
				if (QSBNetworkCRC.scriptCRCCheck)
				{
					var crcmessage = new QSBCRCMessage();
					var list = new List<QSBCRCMessageEntry>();
					foreach (string text in QSBNetworkCRC.singleton.scripts.Keys)
					{
						list.Add(new QSBCRCMessageEntry
						{
							name = text,
							channel = (byte)QSBNetworkCRC.singleton.scripts[text]
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

		private static object s_Sync = new UnityEngine.Object();

		private static bool m_DontListen;

		private bool m_LocalClientActive;

		private List<QSBNetworkConnection> m_LocalConnectionsFakeList = new List<QSBNetworkConnection>();

		private QSBULocalConnectionToClient m_LocalConnection = null;

		private QSBNetworkScene m_NetworkScene;

		private HashSet<int> m_ExternalConnections;

		private ServerSimpleWrapper m_SimpleServerSimple;

		private float m_MaxDelay = 0.1f;

		private HashSet<NetworkInstanceId> m_RemoveList;

		private int m_RemoveListCount;

		private const int k_RemoveListInterval = 100;

		internal static ushort maxPacketSize;

		private static QSBRemovePlayerMessage s_RemovePlayerMessage = new QSBRemovePlayerMessage();

		private class ServerSimpleWrapper : QSBNetworkServerSimple
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