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
		public static List<QNetworkClient> AllClients { get; private set; } = new List<QNetworkClient>();
		public static bool Active { get; private set; }
		public string ServerIp { get; private set; } = "";
		public int ServerPort { get; private set; }
		public QNetworkConnection Connection { get; private set; }
		internal int HostId { get; private set; } = -1;
		public Dictionary<short, QNetworkMessageDelegate> Handlers => _messageHandlers.GetHandlers();
		public int NumChannels => HostTopology.DefaultConfig.ChannelCount;
		public HostTopology HostTopology { get; private set; }

		private const int MaxEventsPerFrame = 500;
		private int _hostPort;
		private int _clientConnectionId = -1;
		private int _statResetTime;
		private static readonly QCRCMessage _crcMessage = new QCRCMessage();
		private readonly QNetworkMessageHandlers _messageHandlers = new QNetworkMessageHandlers();
		private readonly byte[] _msgBuffer;
		private readonly NetworkReader _msgReader;
		protected ConnectState AsyncConnect = ConnectState.None;
		private string _requestedServerHost = "";

		public int HostPort
		{
			get => _hostPort;
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
				_hostPort = value;
			}
		}

		public bool IsConnected => AsyncConnect == ConnectState.Connected;
		public Type NetworkConnectionClass { get; private set; } = typeof(QNetworkConnection);
		public void SetNetworkConnectionClass<T>() where T : QNetworkConnection => NetworkConnectionClass = typeof(T);

		public QNetworkClient()
		{
			_msgBuffer = new byte[65535];
			_msgReader = new NetworkReader(_msgBuffer);
			AddClient(this);
		}

		public QNetworkClient(QNetworkConnection conn)
		{
			_msgBuffer = new byte[65535];
			_msgReader = new NetworkReader(_msgBuffer);
			AddClient(this);
			SetActive(true);
			Connection = conn;
			AsyncConnect = ConnectState.Connected;
			conn.SetHandlers(_messageHandlers);
			RegisterSystemHandlers(false);
		}

		internal void SetHandlers(QNetworkConnection conn) => conn.SetHandlers(_messageHandlers);

		public bool Configure(ConnectionConfig config, int maxConnections)
		{
			var topology = new HostTopology(config, maxConnections);
			return Configure(topology);
		}

		public bool Configure(HostTopology topology)
		{
			HostTopology = topology;
			return true;
		}

		private static bool IsValidIpV6(string address) =>
			address.All(c => c == ':' || (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));

		public void Connect(string serverIp, int serverPort)
		{
			PrepareForConnect();
			this.ServerPort = serverPort;
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				this.ServerIp = serverIp;
				AsyncConnect = ConnectState.Resolved;
			}
			else if (serverIp.Equals("127.0.0.1") || serverIp.Equals("localhost"))
			{
				this.ServerIp = "127.0.0.1";
				AsyncConnect = ConnectState.Resolved;
			}
			else if (serverIp.IndexOf(":") != -1 && IsValidIpV6(serverIp))
			{
				this.ServerIp = serverIp;
				AsyncConnect = ConnectState.Resolved;
			}
			else
			{
				QLog.Log($"Async DNS START:{serverIp}");
				_requestedServerHost = serverIp;
				AsyncConnect = ConnectState.Resolving;
				Dns.BeginGetHostAddresses(serverIp, GetHostAddressesCallback, this);
			}
		}

		private void PrepareForConnect()
		{
			SetActive(true);
			RegisterSystemHandlers(false);
			if (HostTopology == null)
			{
				var connectionConfig = new ConnectionConfig();
				connectionConfig.AddChannel(QosType.ReliableSequenced);
				connectionConfig.AddChannel(QosType.Unreliable);
				connectionConfig.UsePlatformSpecificProtocols = false;
				HostTopology = new HostTopology(connectionConfig, 8);
			}
			HostId = NetworkTransport.AddHost(HostTopology, _hostPort);
		}

		internal static void GetHostAddressesCallback(IAsyncResult ar)
		{
			try
			{
				var array = Dns.EndGetHostAddresses(ar);
				var networkClient = (QNetworkClient)ar.AsyncState;
				if (array.Length == 0)
				{
					QLog.Error($"DNS lookup failed for:{networkClient._requestedServerHost}");
					networkClient.AsyncConnect = ConnectState.Failed;
				}
				else
				{
					networkClient.ServerIp = array[0].ToString();
					networkClient.AsyncConnect = ConnectState.Resolved;
					QLog.Log(
						$"Async DNS Result:{networkClient.ServerIp} for {networkClient._requestedServerHost}: {networkClient.ServerIp}");
				}
			}
			catch (SocketException ex)
			{
				var networkClient2 = (QNetworkClient)ar.AsyncState;
				QLog.Error($"DNS resolution failed: {ex.GetErrorCode()}");
				QLog.Error($"Exception:{ex}");
				networkClient2.AsyncConnect = ConnectState.Failed;
			}
		}

		internal void ContinueConnect()
		{
			_clientConnectionId = NetworkTransport.Connect(HostId, ServerIp, ServerPort, 0, out var b);
			Connection = (QNetworkConnection)Activator.CreateInstance(NetworkConnectionClass);
			Connection.SetHandlers(_messageHandlers);
			Connection.Initialize(ServerIp, HostId, _clientConnectionId, HostTopology);
		}

		public virtual void Disconnect()
		{
			AsyncConnect = ConnectState.Disconnected;
			QClientScene.HandleClientDisconnect(Connection);
			if (Connection != null)
			{
				Connection.Disconnect();
				Connection.Dispose();
				Connection = null;
				if (HostId != -1)
				{
					NetworkTransport.RemoveHost(HostId);
					HostId = -1;
				}
			}
		}

		public bool Send(short msgType, QMessageBase msg)
		{
			bool result;
			if (Connection != null)
			{
				if (AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient Send when not connected to a server");
					result = false;
				}
				else
				{
					result = Connection.Send(msgType, msg);
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
			if (Connection != null)
			{
				if (AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient SendWriter when not connected to a server");
					result = false;
				}
				else
				{
					result = Connection.SendWriter(writer, channelId);
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
			if (Connection != null)
			{
				if (AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient SendBytes when not connected to a server");
					result = false;
				}
				else
				{
					result = Connection.SendBytes(data, numBytes, channelId);
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
			if (Connection != null)
			{
				if (AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient SendUnreliable when not connected to a server");
					result = false;
				}
				else
				{
					result = Connection.SendUnreliable(msgType, msg);
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
			if (Connection != null)
			{
				if (AsyncConnect != ConnectState.Connected)
				{
					QLog.Error("NetworkClient SendByChannel when not connected to a server");
					result = false;
				}
				else
				{
					result = Connection.SendByChannel(msgType, msg, channelId);
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
			if (Connection == null)
			{
				QLog.Warning("SetMaxDelay failed, not connected.");
			}
			else
			{
				Connection.SetMaxDelay(seconds);
			}
		}

		public void Shutdown()
		{
			QLog.Log($"Shutting down client {HostId}");
			if (HostId != -1)
			{
				NetworkTransport.RemoveHost(HostId);
				HostId = -1;
			}
			RemoveClient(this);
			if (AllClients.Count == 0)
			{
				SetActive(false);
			}
		}

		internal virtual void Update()
		{
			if (HostId != -1)
			{
				switch (AsyncConnect)
				{
					case ConnectState.None:
					case ConnectState.Resolving:
					case ConnectState.Disconnected:
						return;
					case ConnectState.Resolved:
						AsyncConnect = ConnectState.Connecting;
						ContinueConnect();
						return;
					case ConnectState.Failed:
						GenerateConnectError(11);
						AsyncConnect = ConnectState.Disconnected;
						return;
				}
				if (Connection != null)
				{
					if ((int)Time.time != _statResetTime)
					{
						Connection.ResetStats();
						_statResetTime = (int)Time.time;
					}
				}
				var num = 0;
				byte b;
				for (; ; )
				{
					var networkEventType = NetworkTransport.ReceiveFromHost(HostId, out var num2, out var channelId, _msgBuffer, (ushort)_msgBuffer.Length, out var numBytes, out b);
					if (Connection != null)
					{
						Connection.LastError = (NetworkError)b;
					}
					switch (networkEventType)
					{
						case NetworkEventType.DataEvent:
							if (b != 0)
							{
								goto Block_11;
							}
							_msgReader.SeekZero();
							Connection.TransportReceive(_msgBuffer, numBytes, channelId);
							break;

						case NetworkEventType.ConnectEvent:
							QLog.Log("Client connected");
							if (b != 0)
							{
								goto Block_10;
							}
							AsyncConnect = ConnectState.Connected;
							Connection.InvokeHandlerNoData(32);
							break;

						case NetworkEventType.DisconnectEvent:
							QLog.Log("Client disconnected");
							AsyncConnect = ConnectState.Disconnected;
							if (b != 0)
							{
								if (b != 6)
								{
									GenerateDisconnectError(b);
								}
							}
							QClientScene.HandleClientDisconnect(Connection);
							Connection?.InvokeHandlerNoData(33);
							break;

						case NetworkEventType.Nothing:
							break;

						default:
							QLog.Error($"Unknown network message type received: {networkEventType}");
							break;
					}
					if (++num >= MaxEventsPerFrame)
					{
						goto Block_17;
					}
					if (HostId == -1)
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
				QLog.Log($"MaxEventsPerFrame hit ({MaxEventsPerFrame})");
				Block_19:
				IL_2C6:
				if (Connection != null && AsyncConnect == ConnectState.Connected)
				{
					Connection.FlushChannels();
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
			var handler = _messageHandlers.GetHandler(34)
						  ?? _messageHandlers.GetHandler(34);
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
					Connection = Connection,
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
			if (Connection != null)
			{
				Connection.GetStatsOut(out numMsgs, out numBufferedMsgs, out numBytes, out lastBufferedPerSecond);
			}
		}

		public void GetStatsIn(out int numMsgs, out int numBytes)
		{
			numMsgs = 0;
			numBytes = 0;
			if (Connection != null)
			{
				Connection.GetStatsIn(out numMsgs, out numBytes);
			}
		}

		public Dictionary<short, QNetworkConnection.PacketStat> GetConnectionStats() =>
			Connection?.PacketStats;

		public void ResetConnectionStats() => Connection?.ResetStats();

		public int GetRTT() =>
			HostId == -1 ? 0 : NetworkTransport.GetCurrentRTT(HostId, _clientConnectionId, out var b);

		internal void RegisterSystemHandlers(bool localClient)
		{
			QClientScene.RegisterSystemHandlers(this, localClient);
			RegisterHandlerSafe(14, OnCRC);
		}

		private void OnCRC(QNetworkMessage netMsg)
		{
			netMsg.ReadMessage(_crcMessage);
			QNetworkCRC.Validate(_crcMessage.scripts, NumChannels);
		}

		public void RegisterHandler(short msgType, QNetworkMessageDelegate handler) => _messageHandlers.RegisterHandler(msgType, handler);

		public void RegisterHandlerSafe(short msgType, QNetworkMessageDelegate handler) => _messageHandlers.RegisterHandlerSafe(msgType, handler);

		public void UnregisterHandler(short msgType) => _messageHandlers.UnregisterHandler(msgType);

		public static Dictionary<short, QNetworkConnection.PacketStat> GetTotalConnectionStats()
		{
			var dictionary = new Dictionary<short, QNetworkConnection.PacketStat>();
			foreach (var networkClient in AllClients)
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

		internal static void AddClient(QNetworkClient client) => AllClients.Add(client);

		internal static bool RemoveClient(QNetworkClient client) => AllClients.Remove(client);

		internal static void UpdateClients()
		{
			for (var i = 0; i < AllClients.Count; i++)
			{
				if (AllClients[i] != null)
				{
					AllClients[i].Update();
				}
				else
				{
					AllClients.RemoveAt(i);
				}
			}
		}

		public static void ShutdownAll()
		{
			while (AllClients.Count != 0)
			{
				AllClients[0].Shutdown();
			}
			AllClients = new List<QNetworkClient>();
			Active = false;
			QClientScene.Shutdown();
		}

		internal static void SetActive(bool state)
		{
			if (!Active && state)
			{
				NetworkTransport.Init();
			}
			Active = state;
		}

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