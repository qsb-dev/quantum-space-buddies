using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	class QSBChannelBuffer : IDisposable
	{
		public QSBChannelBuffer(QSBNetworkConnection conn, int bufferSize, byte cid, bool isReliable, bool isSequenced)
		{
			m_Connection = conn;
			m_MaxPacketSize = bufferSize - 100;
			m_CurrentPacket = new QSBChannelPacket(m_MaxPacketSize, isReliable);
			m_ChannelId = cid;
			m_MaxPendingPacketCount = 16;
			m_IsReliable = isReliable;
			m_AllowFragmentation = isReliable && isSequenced;
			if (isReliable)
			{
				m_PendingPackets = new Queue<QSBChannelPacket>();
				if (s_FreePackets == null)
				{
					s_FreePackets = new List<QSBChannelPacket>();
				}
			}
		}

		public int numMsgsOut { get; private set; }

		public int numBufferedMsgsOut { get; private set; }

		public int numBytesOut { get; private set; }

		public int numMsgsIn { get; private set; }

		public int numBytesIn { get; private set; }

		public int numBufferedPerSecond { get; private set; }

		public int lastBufferedPerSecond { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!m_Disposed)
			{
				if (disposing)
				{
					if (m_PendingPackets != null)
					{
						while (m_PendingPackets.Count > 0)
						{
							pendingPacketCount--;
							var item = m_PendingPackets.Dequeue();
							if (s_FreePackets.Count < 512)
							{
								s_FreePackets.Add(item);
							}
						}
						m_PendingPackets.Clear();
					}
				}
			}
			m_Disposed = true;
		}

		public bool SetOption(ChannelOption option, int value)
		{
			bool result;
			if (option != ChannelOption.MaxPendingBuffers)
			{
				if (option != ChannelOption.AllowFragmentation)
				{
					if (option != ChannelOption.MaxPacketSize)
					{
						result = false;
					}
					else if (!m_CurrentPacket.IsEmpty() || m_PendingPackets.Count > 0)
					{
						if (LogFilter.logError)
						{
							Debug.LogError("Cannot set MaxPacketSize after sending data.");
						}
						result = false;
					}
					else if (value <= 0)
					{
						if (LogFilter.logError)
						{
							Debug.LogError("Cannot set MaxPacketSize less than one.");
						}
						result = false;
					}
					else if (value > m_MaxPacketSize)
					{
						if (LogFilter.logError)
						{
							Debug.LogError("Cannot set MaxPacketSize to greater than the existing maximum (" + m_MaxPacketSize + ").");
						}
						result = false;
					}
					else
					{
						m_CurrentPacket = new QSBChannelPacket(value, m_IsReliable);
						m_MaxPacketSize = value;
						result = true;
					}
				}
				else
				{
					m_AllowFragmentation = value != 0;
					result = true;
				}
			}
			else if (!m_IsReliable)
			{
				result = false;
			}
			else if (value < 0 || value >= 512)
			{
				if (LogFilter.logError)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"Invalid MaxPendingBuffers for channel ",
						m_ChannelId,
						". Must be greater than zero and less than ",
						512
					}));
				}
				result = false;
			}
			else
			{
				m_MaxPendingPacketCount = value;
				result = true;
			}
			return result;
		}

		public void CheckInternalBuffer()
		{
			if (Time.realtimeSinceStartup - m_LastFlushTime > maxDelay && !m_CurrentPacket.IsEmpty())
			{
				SendInternalBuffer();
				m_LastFlushTime = Time.realtimeSinceStartup;
			}
			if (Time.realtimeSinceStartup - m_LastBufferedMessageCountTimer > 1f)
			{
				lastBufferedPerSecond = numBufferedPerSecond;
				numBufferedPerSecond = 0;
				m_LastBufferedMessageCountTimer = Time.realtimeSinceStartup;
			}
		}

		public bool SendWriter(NetworkWriter writer)
		{
			var arraySegment = (ArraySegment<byte>)writer.GetType().GetMethod("AsArraySegment", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(writer, null);
			return SendBytes(arraySegment.Array, arraySegment.Count);
		}

		public bool Send(short msgType, MessageBase msg)
		{
			s_SendWriter.StartMessage(msgType);
			msg.Serialize(s_SendWriter);
			s_SendWriter.FinishMessage();
			numMsgsOut++;
			return SendWriter(s_SendWriter);
		}

		internal bool HandleFragment(NetworkReader reader)
		{
			bool result;
			if (reader.ReadByte() == 0)
			{
				if (!readingFragment)
				{
					fragmentBuffer.SeekZero();
					readingFragment = true;
				}
				var array = reader.ReadBytesAndSize();
				fragmentBuffer.WriteBytes(array, (ushort)array.Length);
				result = false;
			}
			else
			{
				readingFragment = false;
				result = true;
			}
			return result;
		}

		internal bool SendFragmentBytes(byte[] bytes, int bytesToSend)
		{
			var num = 0;
			while (bytesToSend > 0)
			{
				var num2 = Math.Min(bytesToSend, m_MaxPacketSize - 32);
				var array = new byte[num2];
				Array.Copy(bytes, num, array, 0, num2);
				s_FragmentWriter.StartMessage(17);
				s_FragmentWriter.Write(0);
				s_FragmentWriter.WriteBytesFull(array);
				s_FragmentWriter.FinishMessage();
				SendWriter(s_FragmentWriter);
				num += num2;
				bytesToSend -= num2;
			}
			s_FragmentWriter.StartMessage(17);
			s_FragmentWriter.Write(1);
			s_FragmentWriter.FinishMessage();
			SendWriter(s_FragmentWriter);
			return true;
		}

		internal bool SendBytes(byte[] bytes, int bytesToSend)
		{
			bool result;
			if (bytesToSend >= 65535)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ChannelBuffer:SendBytes cannot send packet larger than " + ushort.MaxValue + " bytes");
				}
				result = false;
			}
			else if (bytesToSend <= 0)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("ChannelBuffer:SendBytes cannot send zero bytes");
				}
				result = false;
			}
			else if (bytesToSend > m_MaxPacketSize)
			{
				if (m_AllowFragmentation)
				{
					result = SendFragmentBytes(bytes, bytesToSend);
				}
				else
				{
					if (LogFilter.logError)
					{
						Debug.LogError(string.Concat(new object[]
						{
							"Failed to send big message of ",
							bytesToSend,
							" bytes. The maximum is ",
							m_MaxPacketSize,
							" bytes on channel:",
							m_ChannelId
						}));
					}
					result = false;
				}
			}
			else if (!m_CurrentPacket.HasSpace(bytesToSend))
			{
				if (m_IsReliable)
				{
					if (m_PendingPackets.Count == 0)
					{
						if (!m_CurrentPacket.SendToTransport(m_Connection, m_ChannelId))
						{
							QueuePacket();
						}
						m_CurrentPacket.Write(bytes, bytesToSend);
						result = true;
					}
					else if (m_PendingPackets.Count >= m_MaxPendingPacketCount)
					{
						if (!m_IsBroken)
						{
							if (LogFilter.logError)
							{
								Debug.LogError("ChannelBuffer buffer limit of " + m_PendingPackets.Count + " packets reached.");
							}
						}
						m_IsBroken = true;
						result = false;
					}
					else
					{
						QueuePacket();
						m_CurrentPacket.Write(bytes, bytesToSend);
						result = true;
					}
				}
				else if (!m_CurrentPacket.SendToTransport(m_Connection, m_ChannelId))
				{
					if (LogFilter.logError)
					{
						Debug.Log("ChannelBuffer SendBytes no space on unreliable channel " + m_ChannelId);
					}
					result = false;
				}
				else
				{
					m_CurrentPacket.Write(bytes, bytesToSend);
					result = true;
				}
			}
			else
			{
				m_CurrentPacket.Write(bytes, bytesToSend);
				result = maxDelay != 0f || SendInternalBuffer();
			}
			return result;
		}

		private void QueuePacket()
		{
			pendingPacketCount++;
			m_PendingPackets.Enqueue(m_CurrentPacket);
			m_CurrentPacket = AllocPacket();
		}

		private QSBChannelPacket AllocPacket()
		{
			QSBChannelPacket result;
			if (s_FreePackets.Count == 0)
			{
				result = new QSBChannelPacket(m_MaxPacketSize, m_IsReliable);
			}
			else
			{
				var channelPacket = s_FreePackets[s_FreePackets.Count - 1];
				s_FreePackets.RemoveAt(s_FreePackets.Count - 1);
				channelPacket.Reset();
				result = channelPacket;
			}
			return result;
		}

		private static void FreePacket(QSBChannelPacket packet)
		{
			if (s_FreePackets.Count < 512)
			{
				s_FreePackets.Add(packet);
			}
		}

		public bool SendInternalBuffer()
		{
			bool result;
			if (m_IsReliable && m_PendingPackets.Count > 0)
			{
				while (m_PendingPackets.Count > 0)
				{
					var channelPacket = m_PendingPackets.Dequeue();
					if (!channelPacket.SendToTransport(m_Connection, m_ChannelId))
					{
						m_PendingPackets.Enqueue(channelPacket);
						break;
					}
					pendingPacketCount--;
					FreePacket(channelPacket);
					if (m_IsBroken && m_PendingPackets.Count < m_MaxPendingPacketCount / 2)
					{
						if (LogFilter.logWarn)
						{
							Debug.LogWarning("ChannelBuffer recovered from overflow but data was lost.");
						}
						m_IsBroken = false;
					}
				}
				result = true;
			}
			else
			{
				result = m_CurrentPacket.SendToTransport(m_Connection, m_ChannelId);
			}
			return result;
		}

		private QSBNetworkConnection m_Connection;

		private QSBChannelPacket m_CurrentPacket;

		private float m_LastFlushTime;

		private byte m_ChannelId;

		private int m_MaxPacketSize;

		private bool m_IsReliable;

		private bool m_AllowFragmentation;

		private bool m_IsBroken;

		private int m_MaxPendingPacketCount;

		private const int k_MaxFreePacketCount = 512;

		public const int MaxPendingPacketCount = 16;

		public const int MaxBufferedPackets = 512;

		private Queue<QSBChannelPacket> m_PendingPackets;

		private static List<QSBChannelPacket> s_FreePackets;

		internal static int pendingPacketCount;

		public float maxDelay = 0.01f;

		private float m_LastBufferedMessageCountTimer = Time.realtimeSinceStartup;

		private static NetworkWriter s_SendWriter = new NetworkWriter();

		private static NetworkWriter s_FragmentWriter = new NetworkWriter();

		private const int k_PacketHeaderReserveSize = 100;

		private bool m_Disposed;

		internal QSBNetBuffer fragmentBuffer = new QSBNetBuffer();

		private bool readingFragment = false;
	}
}
