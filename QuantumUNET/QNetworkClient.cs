using QuantumUNET.Logging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET
{
	public class QNetworkClient
	{
		public QNetworkClient()
		{
			m_MsgBuffer = new byte[65535];
			m_MsgReader = new NetworkReader(m_MsgBuffer);
			AddClient(this);
		}

		public QNetworkClient(QNetworkConnection conn)
		{
			m_MsgBuffer = new byte[65535];
			m_MsgReader = new NetworkReader(m_MsgBuffer);
			AddClient(this);
			SetActive(true);
			m_Connection = conn;
			m_AsyncConnect = ConnectState.Connected;
			conn.SetHandlers(m_MessageHandlers);
			RegisterSystemHandlers(false);
		}

		public static List<QNetworkClient> allClients { get; private set; } = new List<QNetworkClient>();

		public static bool active { get; private set; }

		internal void SetHandlers(QNetworkConnection conn) => conn.SetHandlers(m_MessageHandlers);

		public string serverIp { get; private set; } = "";

		public int serverPort { get; private set; }

		public QNetworkConnection connection => m_Connection;

		internal int hostId { get; private set; } = -1;

		public Dictionary<short, QNetworkMessageDelegate> handlers => m_MessageHandlers.GetHandlers();

		public int numChannels => hostTopology.DefaultConfig.ChannelCount;

		public HostTopology hostTopology { get; private set; }

		public int hostPort
		{
			get => m_HostPort;
			set
			{
				if (value < 0)
				{
					throw new ArgumentException("Port must not be a negative number.");
				}
				if (value > 65535)
				{
					throw new ArgumentException("Port must not be greater than 65535.");
				}
				m_HostPort = value;
			}
		}

		public bool isConnected => m_AsyncConnect == ConnectState.Connected;

		public Type networkConnectionClass { get; private set; } = typeof(QNetworkConnection);

		public void SetNetworkConnectionClass<T>() where T : QNetworkConnection => networkConnectionClass = typeof(T);

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

		public bool ReconnectToNewHost(string serverIp, int serverPort)
		{
			bool result;
			if (!active)
			{
				QLog.Error("Reconnect - NetworkClient must be active");
				result = false;
			}
			else if (m_Connection == null)
			{
				QLog.Error("Reconnect - no old connection exists");
				result = false;
			}
			else
			{
				QLog.Log($"NetworkClient Reconnect {serverIp}:{serverPort}");
				QClientScene.HandleClientDisconnect(m_Connection);
				QClientScene.ClearLocalPlayers();
				m_Connection.Disconnect();
				m_Connection = null;
				hostId = NetworkTransport.AddHost(hostTopology, m_HostPort);
				this.serverPort = serverPort;
				if (Application.platform == RuntimePlatform.WebGLPlayer)
				{
					this.serverIp = serverIp;
					m_AsyncConnect = ConnectState.Resolved;
				}
				else if (serverIp.Equals("127.0.0.1") || serverIp.Equals("localhost"))
				{
					this.serverIp = "127.0.0.1";
					m_AsyncConnect = ConnectState.Resolved;
				}
				else
				{
					QLog.Log($"Async DNS START:{serverIp}");
					m_AsyncConnect = ConnectState.Resolving;
					Dns.BeginGetHostAddresses(serverIp, GetHostAddressesCallback, this);
				}
				result = true;
			}
			return result;
		}

		public bool ReconnectToNewHost(EndPoint secureTunnelEndPoint)
		{
			bool result;
			if (!active)
			{
				QLog.Error("Reconnect - NetworkClient must be active");
				result = false;
			}
			else if (m_Connection == null)
			{
				QLog.Error("Reconnect - no old connection exists");
				result = false;
			}
			else
			{
				QLog.Log("NetworkClient Reconnect to remoteSockAddr");
				QClientScene.HandleClientDisconnect(m_Connection);
				QClientScene.ClearLocalPlayers();
				m_Connection.Disconnect();
				m_Connection = null;
				hostId = NetworkTransport.AddHost(hostTopology, m_HostPort);
				if (secureTunnelEndPoint == null)
				{
					QLog.Error("Reconnect failed: null endpoint passed in");
					m_AsyncConnect = ConnectState.Failed;
					result = false;
				}
				else if (secureTunnelEndPoint.AddressFamily != AddressFamily.InterNetwork && secureTunnelEndPoint.AddressFamily != AddressFamily.InterNetworkV6)
				{
					QLog.Error("Reconnect failed: Endpoint AddressFamily must be either InterNetwork or InterNetworkV6");
					m_AsyncConnect = ConnectState.Failed;
					result = false;
				}
				else
				{
					var fullName = secureTunnelEndPoint.GetType().FullName;
					if (fullName == "System.Net.IPEndPoint")
					{
						var ipendPoint = (IPEndPoint)secureTunnelEndPoint;
						Connect(ipendPoint.Address.ToString(), ipendPoint.Port);
						result = m_AsyncConnect != ConnectState.Failed;
					}
					else if (fullName != "UnityEngine.XboxOne.XboxOneEndPoint" && fullName != "UnityEngine.PS4.SceEndPoint")
					{
						QLog.Error("Reconnect failed: invalid Endpoint (not IPEndPoint or XboxOneEndPoint or SceEndPoint)");
						m_AsyncConnect = ConnectState.Failed;
						result = false;
					}
					else
					{
						m_RemoteEndPoint = secureTunnelEndPoint;
						m_AsyncConnect = ConnectState.Connecting;
						byte b;
						try
						{
							m_ClientConnectionId = NetworkTransport.ConnectEndPoint(hostId, m_RemoteEndPoint, 0, out b);
						}
						catch (Exception arg)
						{
							QLog.Error($"Reconnect failed: Exception when trying to connect to EndPoint: {arg}");
							m_AsyncConnect = ConnectState.Failed;
							return false;
						}
						if (m_ClientConnectionId == 0)
						{
							QLog.Error($"Reconnect failed: Unable to connect to EndPoint ({b})");
							m_AsyncConnect = ConnectState.Failed;
							result = false;
						}
						else
						{
							m_Connection = (QNetworkConnection)Activator.CreateInstance(networkConnectionClass);
							m_Connection.SetHandlers(m_MessageHandlers);
							m_Connection.Initialize(serverIp, hostId, m_ClientConnectionId, hostTopology);
							result = true;
						}
					}
				}
			}
			return result;
		}

		public void ConnectWithSimulator(string serverIp, int serverPort, int latency, float packetLoss)
		{
			m_UseSimulator = true;
			m_SimulatedLatency = latency;
			m_PacketLoss = packetLoss;
			Connect(serverIp, serverPort);
		}

		private static bool IsValidIpV6(string address) =>
			address.All(c => c == ':' || (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));

		public void Connect(string serverIp, int serverPort)
		{
			PrepareForConnect();
			this.serverPort = serverPort;
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				this.serverIp = serverIp;
				m_AsyncConnect = ConnectState.Resolved;
			}
			else if (serverIp.Equals("127.0.0.1") || serverIp.Equals("localhost"))
			{
				this.serverIp = "127.0.0.1";
				m_AsyncConnect = ConnectState.Resolved;
			}
			else if (serverIp.IndexOf(":") != -1 && IsValidIpV6(serverIp))
			{
				this.serverIp = serverIp;
				m_AsyncConnect = ConnectState.Resolved;
			}
			else
			{
				QLog.Log($"Async DNS START:{serverIp}");
				m_RequestedServerHost = serverIp;
				m_AsyncConnect = ConnectState.Resolving;
				Dns.BeginGetHostAddresses(serverIp, GetHostAddressesCallback, this);
			}
		}

		public void Connect(EndPoint secureTunnelEndPoint)
		{
			PrepareForConnect();
			QLog.Log("Client Connect to remoteSockAddr");
			if (secureTunnelEndPoint == null)
			{
				QLog.Error("Connect failed: null endpoint passed in");
				m_AsyncConnect = ConnectState.Failed;
			}
			else if (secureTunnelEndPoint.AddressFamily != AddressFamily.InterNetwork && secureTunnelEndPoint.AddressFamily != AddressFamily.InterNetworkV6)
			{
				QLog.Error("Connect failed: Endpoint AddressFamily must be either InterNetwork or InterNetworkV6");
				m_AsyncConnect = ConnectState.Failed;
			}
			else
			{
				var fullName = secureTunnelEndPoint.GetType().FullName;
				if (fullName == "System.Net.IPEndPoint")
				{
					var ipendPoint = (IPEndPoint)secureTunnelEndPoint;
					Connect(ipendPoint.Address.ToString(), ipendPoint.Port);
				}
				else if (fullName != "UnityEngine.XboxOne.XboxOneEndPoint" && fullName != "UnityEngine.PS4.SceEndPoint" && fullName != "UnityEngine.PSVita.SceEndPoint")
				{
					QLog.Error("Connect failed: invalid Endpoint (not IPEndPoint or XboxOneEndPoint or SceEndPoint)");
					m_AsyncConnect = ConnectState.Failed;
				}
				else
				{
					byte b = 0;
					m_RemoteEndPoint = secureTunnelEndPoint;
					m_AsyncConnect = ConnectState.Connecting;
					try
					{
						m_ClientConnectionId = NetworkTransport.ConnectEndPoint(hostId, m_RemoteEndPoint, 0, out b);
					}
					catch (Exception arg)
					{
						QLog.Error($"Connect failed: Exception when trying to connect to EndPoint: {arg}");
						m_AsyncConnect = ConnectState.Failed;
						return;
					}
					if (m_ClientConnectionId == 0)
					{
						QLog.Error($"Connect failed: Unable to connect to EndPoint ({b})");
						m_AsyncConnect = ConnectState.Failed;
					}
					else
					{
						m_Connection = (QNetworkConnection)Activator.CreateInstance(networkConnectionClass);
						m_Connection.SetHandlers(m_MessageHandlers);
						m_Connection.Initialize(serverIp, hostId, m_ClientConnectionId, hostTopology);
					}
				}
			}
		}

		private void PrepareForConnect()
		{
			SetActive(true);
			RegisterSystemHandlers(false);
			if (hostTopology == null)
			{
				var connectionConfig = new ConnectionConfig();
				connectionConfig.AddChannel(QosType.ReliableSequenced);
				connectionConfig.AddChannel(QosType.Unreliable);
				connectionConfig.UsePlatformSpecificProtocols = false;
				hostTopology = new HostTopology(connectionConfig, 8);
			}
			if (m_UseSimulator)
			{
				var num = m_SimulatedLatency / 3 - 1;
				if (num < 1)
				{
					num = 1;
				}
				var num2 = m_SimulatedLatency * 3;
				QLog.Log($"AddHost Using Simulator {num}/{num2}");
				hostId = NetworkTransport.AddHostWithSimulator(hostTopology, num, num2, m_HostPort);
			}
			else
			{
				hostId = NetworkTransport.AddHost(hostTopology, m_HostPort);
			}
		}

		internal static void GetHostAddressesCallback(IAsyncResult ar)
		{
			try
			{
				var array = Dns.EndGetHostAddresses(ar);
				var networkClient = (QNetworkClient)ar.AsyncState;
				if (array.Length == 0)
				{
					QLog.Error($"DNS lookup failed for:{networkClient.m_RequestedServerHost}");
					networkClient.m_AsyncConnect = ConnectState.Failed;
				}
				else
				{
					networkClient.serverIp = array[0].ToString();
					networkClient.m_AsyncConnect = ConnectState.Resolved;
					QLog.Log(
						$"Async DNS Result:{networkClient.serverIp} for {networkClient.m_RequestedServerHost}: {networkClient.serverIp}");
				}
			}
			catch (SocketException ex)
			{
				var networkClient2 = (QNetworkClient)ar.AsyncState;
				QLog.Error($"DNS resolution failed: {ex.GetErrorCode()}");
				QLog.Error($"Exception:{ex}");
				networkClient2.m_AsyncConnect = ConnectState.Failed;
			}
		}

		internal void ContinueConnect()
		{
			if (m_UseSimulator)
			{
				var num = m_SimulatedLatency / 3;
				if (num < 1)
				{
					num = 1;
				}
				QLog.Log(
					$"Connect Using Simulator {m_SimulatedLatency / 3}/{m_SimulatedLatency}");
				var conf = new ConnectionSimulatorConfig(num, m_SimulatedLatency, num, m_SimulatedLatency, m_PacketLoss);
				m_ClientConnectionId = NetworkTransport.ConnectWithSimulator(hostId, serverIp, serverPort, 0, out var b, conf);
			}
			else
			{
				m_ClientConnectionId = NetworkTransport.Connect(hostId, serverIp, serverPort, 0, out var b);
			}
			m_Connection = (QNetworkConnection)Activator.CreateInstance(networkConnectionClass);
			m_Connection.SetHandlers(m_MessageHandlers);
			m_Connection.Initialize(serverIp, hostId, m_ClientConnectionId, hostTopology);
		}

		public virtual void Disconnect()
		{
			m_AsyncConnect = ConnectState.Disconnected;
			QClientScene.HandleClientDisconnect(m_Connection);
			if (m_Connection != null)
			{
				m_Connection.Disconnect();
				m_Connection.Dispose();
				m_Connection = null;
				if (hostId != -1)
				{
					NetworkTransport.RemoveHost(hostId);
					hostId = -1;
				}
			}
		}

		public bool Send(short msgType, QMessageBase msg)
		{
			bool result;
			if (m_Connection != null)
			{
				if (m_AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient Send when not connected to a server");
					result = false;
				}
				else
				{
					result = m_Connection.Send(msgType, msg);
				}
			}
			else
			{
				QLog.Error("NetworkClient Send with no connection");
				result = false;
			}
			return result;
		}

		public bool SendWriter(QNetworkWriter writer, int channelId)
		{
			bool result;
			if (m_Connection != null)
			{
				if (m_AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient SendWriter when not connected to a server");
					result = false;
				}
				else
				{
					result = m_Connection.SendWriter(writer, channelId);
				}
			}
			else
			{
				QLog.Error("NetworkClient SendWriter with no connection");
				result = false;
			}
			return result;
		}

		public bool SendBytes(byte[] data, int numBytes, int channelId)
		{
			bool result;
			if (m_Connection != null)
			{
				if (m_AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient SendBytes when not connected to a server");
					result = false;
				}
				else
				{
					result = m_Connection.SendBytes(data, numBytes, channelId);
				}
			}
			else
			{
				QLog.Error("NetworkClient SendBytes with no connection");
				result = false;
			}
			return result;
		}

		public bool SendUnreliable(short msgType, QMessageBase msg)
		{
			bool result;
			if (m_Connection != null)
			{
				if (m_AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient SendUnreliable when not connected to a server");
					result = false;
				}
				else
				{
					result = m_Connection.SendUnreliable(msgType, msg);
				}
			}
			else
			{
				QLog.Error("NetworkClient SendUnreliable with no connection");
				result = false;
			}
			return result;
		}

		public bool SendByChannel(short msgType, QMessageBase msg, int channelId)
		{
			bool result;
			if (m_Connection != null)
			{
				if (m_AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient SendByChannel when not connected to a server");
					result = false;
				}
				else
				{
					result = m_Connection.SendByChannel(msgType, msg, channelId);
				}
			}
			else
			{
				QLog.Error("NetworkClient SendByChannel with no connection");
				result = false;
			}
			return result;
		}

		public void SetMaxDelay(float seconds)
		{
			if (m_Connection == null)
			{
				QLog.Warning("SetMaxDelay failed, not connected.");
			}
			else
			{
				m_Connection.SetMaxDelay(seconds);
			}
		}

		public void Shutdown()
		{
			QLog.Log($"Shutting down client {hostId}");
			if (hostId != -1)
			{
				NetworkTransport.RemoveHost(hostId);
				hostId = -1;
			}
			RemoveClient(this);
			if (allClients.Count == 0)
			{
				SetActive(false);
			}
		}

		internal virtual void Update()
		{
			if (hostId != -1)
			{
				switch (m_AsyncConnect)
				{
					case ConnectState.None:
					case ConnectState.Resolving:
					case ConnectState.Disconnected:
						return;
					case ConnectState.Resolved:
						m_AsyncConnect = ConnectState.Connecting;
						ContinueConnect();
						return;
					case ConnectState.Failed:
						GenerateConnectError(11);
						m_AsyncConnect = ConnectState.Disconnected;
						return;
				}
				if (m_Connection != null)
				{
					if ((int)Time.time != m_StatResetTime)
					{
						m_Connection.ResetStats();
						m_StatResetTime = (int)Time.time;
					}
				}
				var num = 0;
				byte b;
				for (; ; )
				{
					var networkEventType = NetworkTransport.ReceiveFromHost(hostId, out var num2, out var channelId, m_MsgBuffer, (ushort)m_MsgBuffer.Length, out var numBytes, out b);
					if (m_Connection != null)
					{
						m_Connection.LastError = (NetworkError)b;
					}
					switch (networkEventType)
					{
						case NetworkEventType.DataEvent:
							if (b != 0)
							{
								goto Block_11;
							}
							m_MsgReader.SeekZero();
							m_Connection.TransportReceive(m_MsgBuffer, numBytes, channelId);
							break;

						case NetworkEventType.ConnectEvent:
							QLog.Log("Client connected");
							if (b != 0)
							{
								goto Block_10;
							}
							m_AsyncConnect = ConnectState.Connected;
							m_Connection.InvokeHandlerNoData(32);
							break;

						case NetworkEventType.DisconnectEvent:
							QLog.Log("Client disconnected");
							m_AsyncConnect = ConnectState.Disconnected;
							if (b != 0)
							{
								if (b != 6)
								{
									GenerateDisconnectError(b);
								}
							}
							QClientScene.HandleClientDisconnect(m_Connection);
							m_Connection?.InvokeHandlerNoData(33);
							break;

						case NetworkEventType.Nothing:
							break;

						default:
							QLog.Error($"Unknown network message type received: {networkEventType}");
							break;
					}
					if (++num >= 500)
					{
						goto Block_17;
					}
					if (hostId == -1)
					{
						goto Block_19;
					}
					if (networkEventType == NetworkEventType.Nothing)
					{
						goto IL_2C6;
					}
				}
				Block_10:
				GenerateConnectError(b);
				return;
				Block_11:
				GenerateDataError(b);
				return;
				Block_17:
				QLog.Log($"MaxEventsPerFrame hit ({500})");
				Block_19:
				IL_2C6:
				if (m_Connection != null && m_AsyncConnect == ConnectState.Connected)
				{
					m_Connection.FlushChannels();
				}
			}
		}

		private void GenerateConnectError(int error)
		{
			QLog.Error($"UNet Client Error Connect Error: {error}");
			GenerateError(error);
		}

		private void GenerateDataError(int error)
		{
			QLog.Error($"UNet Client Data Error: {(NetworkError)error}");
			GenerateError(error);
		}

		private void GenerateDisconnectError(int error)
		{
			QLog.Error($"UNet Client Disconnect Error: {(NetworkError)error}");
			GenerateError(error);
		}

		private void GenerateError(int error)
		{
			var handler = m_MessageHandlers.GetHandler(34)
						  ?? m_MessageHandlers.GetHandler(34);
			if (handler != null)
			{
				var errorMessage = new QErrorMessage
				{
					errorCode = error
				};
				var buffer = new byte[200];
				var writer = new QNetworkWriter(buffer);
				errorMessage.Serialize(writer);
				var reader = new QNetworkReader(buffer);
				handler(new QNetworkMessage
				{
					MsgType = 34,
					Reader = reader,
					Connection = m_Connection,
					ChannelId = 0
				});
			}
		}

		public void GetStatsOut(out int numMsgs, out int numBufferedMsgs, out int numBytes, out int lastBufferedPerSecond)
		{
			numMsgs = 0;
			numBufferedMsgs = 0;
			numBytes = 0;
			lastBufferedPerSecond = 0;
			if (m_Connection != null)
			{
				m_Connection.GetStatsOut(out numMsgs, out numBufferedMsgs, out numBytes, out lastBufferedPerSecond);
			}
		}

		public void GetStatsIn(out int numMsgs, out int numBytes)
		{
			numMsgs = 0;
			numBytes = 0;
			if (m_Connection != null)
			{
				m_Connection.GetStatsIn(out numMsgs, out numBytes);
			}
		}

		public Dictionary<short, QNetworkConnection.PacketStat> GetConnectionStats() =>
			m_Connection?.PacketStats;

		public void ResetConnectionStats() => m_Connection?.ResetStats();

		public int GetRTT() =>
			hostId == -1 ? 0 : NetworkTransport.GetCurrentRTT(hostId, m_ClientConnectionId, out var b);

		internal void RegisterSystemHandlers(bool localClient)
		{
			QClientScene.RegisterSystemHandlers(this, localClient);
			RegisterHandlerSafe(14, OnCRC);
		}

		private void OnCRC(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(s_CRCMessage);
			QNetworkCRC.Validate(s_CRCMessage.scripts, numChannels);
		}

		public void RegisterHandler(short msgType, QNetworkMessageDelegate handler) => m_MessageHandlers.RegisterHandler(msgType, handler);

		public void RegisterHandlerSafe(short msgType, QNetworkMessageDelegate handler) => m_MessageHandlers.RegisterHandlerSafe(msgType, handler);

		public void UnregisterHandler(short msgType) => m_MessageHandlers.UnregisterHandler(msgType);

		public static Dictionary<short, QNetworkConnection.PacketStat> GetTotalConnectionStats()
		{
			var dictionary = new Dictionary<short, QNetworkConnection.PacketStat>();
			foreach (var networkClient in allClients)
			{
				var connectionStats = networkClient.GetConnectionStats();
				foreach (var key in connectionStats.Keys)
				{
					if (dictionary.ContainsKey(key))
					{
						var packetStat = dictionary[key];
						packetStat.count += connectionStats[key].count;
						packetStat.bytes += connectionStats[key].bytes;
						dictionary[key] = packetStat;
					}
					else
					{
						dictionary[key] = new QNetworkConnection.PacketStat(connectionStats[key]);
					}
				}
			}
			return dictionary;
		}

		internal static void AddClient(QNetworkClient client) => allClients.Add(client);

		internal static bool RemoveClient(QNetworkClient client) => allClients.Remove(client);

		internal static void UpdateClients()
		{
			for (var i = 0; i < allClients.Count; i++)
			{
				if (allClients[i] != null)
				{
					allClients[i].Update();
				}
				else
				{
					allClients.RemoveAt(i);
				}
			}
		}

		public static void ShutdownAll()
		{
			while (allClients.Count != 0)
			{
				allClients[0].Shutdown();
			}
			allClients = new List<QNetworkClient>();
			active = false;
			QClientScene.Shutdown();
		}

		internal static void SetActive(bool state)
		{
			if (!active && state)
			{
				NetworkTransport.Init();
			}
			active = state;
		}

		private const int k_MaxEventsPerFrame = 500;
		private int m_HostPort;

		private bool m_UseSimulator;

		private int m_SimulatedLatency;

		private float m_PacketLoss;
		private int m_ClientConnectionId = -1;

		private int m_StatResetTime;

		private EndPoint m_RemoteEndPoint;

		private static readonly QCRCMessage s_CRCMessage = new QCRCMessage();

		private readonly QNetworkMessageHandlers m_MessageHandlers = new QNetworkMessageHandlers();

		protected QNetworkConnection m_Connection;

		private readonly byte[] m_MsgBuffer;

		private readonly NetworkReader m_MsgReader;

		protected ConnectState m_AsyncConnect = ConnectState.None;

		private string m_RequestedServerHost = "";

		protected enum ConnectState
		{
			None,
			Resolving,
			Resolved,
			Connecting,
			Connected,
			Disconnected,
			Failed
		}
	}
}