using QuantumUNET.Components;
using QuantumUNET.Logging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET
{
	public class QNetworkConnection : IDisposable
	{
		public QNetworkConnection() => m_Writer = new QNetworkWriter();

		internal HashSet<QNetworkIdentity> VisList { get; } = new HashSet<QNetworkIdentity>();

		public List<QPlayerController> PlayerControllers { get; } = new List<QPlayerController>();

		public HashSet<NetworkInstanceId> ClientOwnedObjects { get; private set; }

		public bool isConnected => hostId != -1;

		public NetworkError LastError { get; internal set; }

		internal Dictionary<short, PacketStat> PacketStats { get; } = new Dictionary<short, PacketStat>();

		public virtual void Initialize(string networkAddress, int networkHostId, int networkConnectionId, HostTopology hostTopology)
		{
			m_Writer = new QNetworkWriter();
			address = networkAddress;
			hostId = networkHostId;
			connectionId = networkConnectionId;
			var channelCount = hostTopology.DefaultConfig.ChannelCount;
			var packetSize = (int)hostTopology.DefaultConfig.PacketSize;
			if (hostTopology.DefaultConfig.UsePlatformSpecificProtocols && Application.platform != RuntimePlatform.PS4 && Application.platform != RuntimePlatform.PSP2)
			{
				throw new ArgumentOutOfRangeException("Platform specific protocols are not supported on this platform");
			}

			m_Channels = new QChannelBuffer[channelCount];
			for (var i = 0; i < channelCount; i++)
			{
				var channelQOS = hostTopology.DefaultConfig.Channels[i];
				var bufferSize = packetSize;
				if (channelQOS.QOS is QosType.ReliableFragmented or QosType.UnreliableFragmented)
				{
					bufferSize = hostTopology.DefaultConfig.FragmentSize * 128;
				}

				m_Channels[i] = new QChannelBuffer(this, bufferSize, (byte)i, IsReliableQoS(channelQOS.QOS), IsSequencedQoS(channelQOS.QOS));
			}
		}

		~QNetworkConnection()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!m_Disposed && m_Channels != null)
			{
				foreach (var channel in m_Channels)
				{
					channel.Dispose();
				}
			}

			m_Channels = null;
			if (ClientOwnedObjects != null)
			{
				foreach (var netId in ClientOwnedObjects)
				{
					var gameObject = QNetworkServer.FindLocalObject(netId);
					if (gameObject != null)
					{
						gameObject.GetComponent<QNetworkIdentity>().ClearClientOwner();
					}
				}
			}

			ClientOwnedObjects = null;
			m_Disposed = true;
		}

		private static bool IsSequencedQoS(QosType qos) => qos is QosType.ReliableSequenced or QosType.UnreliableSequenced;

		private static bool IsReliableQoS(QosType qos) => qos is QosType.Reliable or QosType.ReliableFragmented or QosType.ReliableSequenced or QosType.ReliableStateUpdate;

		public bool SetChannelOption(int channelId, ChannelOption option, int value) => m_Channels != null && channelId >= 0 && channelId < m_Channels.Length && m_Channels[channelId].SetOption(option, value);

		public void Disconnect()
		{
			address = "";
			isReady = false;
			QClientScene.HandleClientDisconnect(this);
			if (hostId != -1)
			{
				NetworkTransport.Disconnect(hostId, connectionId, out var b);
				RemoveObservers();
			}
		}

		internal void SetHandlers(QNetworkMessageHandlers handlers)
		{
			m_MessageHandlers = handlers;
			m_MessageHandlersDict = handlers.GetHandlers();
		}

		public bool CheckHandler(short msgType) => m_MessageHandlersDict.ContainsKey(msgType);

		public bool InvokeHandlerNoData(short msgType) => InvokeHandler(msgType, null, 0);

		public bool InvokeHandler(short msgType, QNetworkReader reader, int channelId)
		{
			bool result;
			if (m_MessageHandlersDict.ContainsKey(msgType))
			{
				m_MessageInfo.MsgType = msgType;
				m_MessageInfo.Connection = this;
				m_MessageInfo.Reader = reader;
				m_MessageInfo.ChannelId = channelId;
				var networkMessageDelegate = m_MessageHandlersDict[msgType];
				if (networkMessageDelegate == null)
				{
					result = false;
				}
				else
				{
					networkMessageDelegate(m_MessageInfo);
					result = true;
				}
			}
			else
			{
				result = false;
			}

			return result;
		}

		public bool InvokeHandler(QNetworkMessage netMsg)
		{
			bool result;
			if (m_MessageHandlersDict.ContainsKey(netMsg.MsgType))
			{
				var networkMessageDelegate = m_MessageHandlersDict[netMsg.MsgType];
				networkMessageDelegate(netMsg);
				result = true;
			}
			else
			{
				result = false;
			}

			return result;
		}

		internal void HandleFragment(QNetworkReader reader, int channelId)
		{
			if (channelId >= 0 && channelId < m_Channels.Length)
			{
				var channelBuffer = m_Channels[channelId];
				if (channelBuffer.HandleFragment(reader))
				{
					var networkReader = new QNetworkReader(channelBuffer._fragmentBuffer.AsArraySegment().Array);
					networkReader.ReadInt16();
					var msgType = networkReader.ReadInt16();
					InvokeHandler(msgType, networkReader, channelId);
				}
			}
		}

		public void RegisterHandler(short msgType, QNetworkMessageDelegate handler) => m_MessageHandlers.RegisterHandler(msgType, handler);

		public void UnregisterHandler(short msgType) => m_MessageHandlers.UnregisterHandler(msgType);

		internal void SetPlayerController(QPlayerController player)
		{
			while (player.PlayerControllerId >= PlayerControllers.Count)
			{
				PlayerControllers.Add(new QPlayerController());
			}

			PlayerControllers[player.PlayerControllerId] = player;
		}

		internal void RemovePlayerController(short playerControllerId)
		{
			for (var i = PlayerControllers.Count; i >= 0; i--)
			{
				if (playerControllerId == i && playerControllerId == PlayerControllers[i].PlayerControllerId)
				{
					PlayerControllers[i] = new QPlayerController();
					return;
				}
			}

			QLog.Error($"RemovePlayer player at playerControllerId {playerControllerId} not found");
		}

		internal bool GetPlayerController(short playerControllerId, out QPlayerController playerController)
		{
			playerController = null;
			bool result;
			if (PlayerControllers.Count > 0)
			{
				foreach (var controller in PlayerControllers)
				{
					if (controller.IsValid && controller.PlayerControllerId == playerControllerId)
					{
						playerController = controller;
						return true;
					}
				}

				result = false;
			}
			else
			{
				result = false;
			}

			return result;
		}

		public void FlushChannels()
		{
			if (m_Channels != null)
			{
				foreach (var channel in m_Channels)
				{
					channel.CheckInternalBuffer();
				}
			}
		}

		public void SetMaxDelay(float seconds)
		{
			if (m_Channels != null)
			{
				foreach (var channel in m_Channels)
				{
					channel.MaxDelay = seconds;
				}
			}
		}

		public virtual bool Send(short msgType, QMessageBase msg) => SendByChannel(msgType, msg, 0);

		public virtual bool SendUnreliable(short msgType, QMessageBase msg) => SendByChannel(msgType, msg, 1);

		public virtual bool SendByChannel(short msgType, QMessageBase msg, int channelId)
		{
			m_Writer.StartMessage(msgType);
			msg.Serialize(m_Writer);
			m_Writer.FinishMessage();
			return SendWriter(m_Writer, channelId);
		}

		public virtual bool SendBytes(byte[] bytes, int numBytes, int channelId) => CheckChannel(channelId) && m_Channels[channelId].SendBytes(bytes, numBytes);

		public virtual bool SendWriter(QNetworkWriter writer, int channelId) => CheckChannel(channelId) && m_Channels[channelId].SendWriter(writer);

		private void LogSend(byte[] bytes)
		{
			var networkReader = new QNetworkReader(bytes);
			var num = networkReader.ReadUInt16();
			var num2 = networkReader.ReadUInt16();
			var stringBuilder = new StringBuilder();
			for (var i = 4; i < 4 + num; i++)
			{
				stringBuilder.AppendFormat("{0:X2}", bytes[i]);
				if (i > 150)
				{
					break;
				}
			}

			QLog.Log(
				$"ConnectionSend con:{connectionId} bytes:{num} msgId:{num2} {stringBuilder}");
		}

		private bool CheckChannel(int channelId)
		{
			bool result;
			if (m_Channels == null)
			{
				QLog.Warning($"Channels not initialized sending on id '{channelId}");
				result = false;
			}
			else if (channelId < 0 || channelId >= m_Channels.Length)
			{
				QLog.Error(
					$"Invalid channel when sending buffered data, '{channelId}'. Current channel count is {m_Channels.Length}");
				result = false;
			}
			else
			{
				result = true;
			}

			return result;
		}

		public void ResetStats()
		{
		}

		protected void HandleBytes(byte[] buffer, int receivedSize, int channelId)
		{
			var reader = new QNetworkReader(buffer);
			HandleReader(reader, receivedSize, channelId);
		}

		protected void HandleReader(QNetworkReader reader, int receivedSize, int channelId)
		{
			while (reader.Position < receivedSize)
			{
				var num = reader.ReadUInt16();
				var num2 = reader.ReadInt16();
				var array = reader.ReadBytes(num);
				var reader2 = new QNetworkReader(array);
				QNetworkMessageDelegate networkMessageDelegate = null;
				if (m_MessageHandlersDict.ContainsKey(num2))
				{
					networkMessageDelegate = m_MessageHandlersDict[num2];
				}

				if (networkMessageDelegate == null)
				{
					QLog.Error($"Unknown message ID {num2} connId:{connectionId}");
					break;
				}

				m_NetMsg.MsgType = num2;
				m_NetMsg.Reader = reader2;
				m_NetMsg.Connection = this;
				m_NetMsg.ChannelId = channelId;
				networkMessageDelegate(m_NetMsg);
				lastMessageTime = Time.time;
			}
		}

		public override string ToString() =>
			$"hostId: {hostId} connectionId: {connectionId} isReady: {isReady} channel count: {m_Channels?.Length ?? 0}";

		internal void AddToVisList(QNetworkIdentity uv)
		{
			VisList.Add(uv);
			QNetworkServer.ShowForConnection(uv, this);
		}

		internal void RemoveFromVisList(QNetworkIdentity uv, bool isDestroyed)
		{
			VisList.Remove(uv);
			if (!isDestroyed)
			{
				QNetworkServer.HideForConnection(uv, this);
			}
		}

		internal void RemoveObservers()
		{
			foreach (var networkIdentity in VisList)
			{
				networkIdentity.RemoveObserverInternal(this);
			}

			VisList.Clear();
		}

		public virtual void TransportReceive(byte[] bytes, int numBytes, int channelId) => HandleBytes(bytes, numBytes, channelId);

		public virtual bool TransportSend(byte[] bytes, int numBytes, int channelId, out byte error) => NetworkTransport.Send(hostId, connectionId, channelId, bytes, numBytes, out error);

		internal void AddOwnedObject(QNetworkIdentity obj)
		{
			if (ClientOwnedObjects == null)
			{
				ClientOwnedObjects = new HashSet<NetworkInstanceId>();
			}

			ClientOwnedObjects.Add(obj.NetId);
		}

		internal void RemoveOwnedObject(QNetworkIdentity obj) => ClientOwnedObjects?.Remove(obj.NetId);

		internal static void OnFragment(QNetworkMessage netMsg) => netMsg.Connection.HandleFragment(netMsg.Reader, netMsg.ChannelId);

		private QChannelBuffer[] m_Channels;
		private readonly QNetworkMessage m_NetMsg = new();
		private QNetworkWriter m_Writer;

		private Dictionary<short, QNetworkMessageDelegate> m_MessageHandlersDict;

		private QNetworkMessageHandlers m_MessageHandlers;
		private readonly QNetworkMessage m_MessageInfo = new();

		private const int k_MaxMessageLogSize = 150;
		public int hostId = -1;

		public int connectionId = -1;

		public bool isReady;

		public string address;

		public float lastMessageTime;

		public bool logNetworkMessages = false;
		private bool m_Disposed;

		public class PacketStat
		{
			public PacketStat()
			{
				msgType = 0;
				count = 0;
				bytes = 0;
			}

			public PacketStat(PacketStat s)
			{
				msgType = s.msgType;
				count = s.count;
				bytes = s.bytes;
			}

			public override string ToString() => $"{QMsgType.MsgTypeToString(msgType)}: count={count} bytes={bytes}";

			public short msgType;

			public int count;

			public int bytes;
		}
	}
}