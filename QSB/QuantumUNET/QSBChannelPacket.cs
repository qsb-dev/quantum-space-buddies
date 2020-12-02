using System;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	struct QSBChannelPacket
	{
		public QSBChannelPacket(int packetSize, bool isReliable)
		{
			this.m_Position = 0;
			this.m_Buffer = new byte[packetSize];
			this.m_IsReliable = isReliable;
		}

		public void Reset()
		{
			this.m_Position = 0;
		}

		public bool IsEmpty()
		{
			return this.m_Position == 0;
		}

		public void Write(byte[] bytes, int numBytes)
		{
			Array.Copy(bytes, 0, this.m_Buffer, this.m_Position, numBytes);
			this.m_Position += numBytes;
		}

		public bool HasSpace(int numBytes)
		{
			return this.m_Position + numBytes <= this.m_Buffer.Length;
		}

		public bool SendToTransport(QSBNetworkConnection conn, int channelId)
		{
			bool result = true;
			byte b;
			if (!conn.TransportSend(this.m_Buffer, (int)((ushort)this.m_Position), channelId, out b))
			{
				if (!this.m_IsReliable || b != 4)
				{
					Debug.LogError(string.Concat(new object[]
					{
						"Failed to send internal buffer channel:",
						channelId,
						" bytesToSend:",
						this.m_Position
					}));
					result = false;
				}
			}
			if (b != 0)
			{
				if (this.m_IsReliable && b == 4)
				{
					return false;
				}
				Debug.LogError(string.Concat(new object[]
				{
					"Send Error: ",
					(NetworkError)b,
					" channel:",
					channelId,
					" bytesToSend:",
					this.m_Position
				}));
				result = false;
			}
			this.m_Position = 0;
			return result;
		}

		private int m_Position;

		private byte[] m_Buffer;

		private bool m_IsReliable;
	}
}
