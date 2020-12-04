using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBNetworkWriter
	{
		public QSBNetworkWriter()
		{
			this.m_Buffer = new QSBNetBuffer();
			if (s_Encoding == null)
			{
				s_Encoding = new UTF8Encoding();
				s_StringWriteBuffer = new byte[32768];
			}
		}

		public QSBNetworkWriter(byte[] buffer)
		{
			this.m_Buffer = new QSBNetBuffer(buffer);
			if (s_Encoding == null)
			{
				s_Encoding = new UTF8Encoding();
				s_StringWriteBuffer = new byte[32768];
			}
		}

		public short Position
		{
			get
			{
				return (short)this.m_Buffer.Position;
			}
		}

		public byte[] ToArray()
		{
			byte[] array = new byte[this.m_Buffer.AsArraySegment().Count];
			Array.Copy(this.m_Buffer.AsArraySegment().Array, array, this.m_Buffer.AsArraySegment().Count);
			return array;
		}

		public byte[] AsArray()
		{
			return this.AsArraySegment().Array;
		}

		internal ArraySegment<byte> AsArraySegment()
		{
			return this.m_Buffer.AsArraySegment();
		}

		public void WritePackedUInt32(uint value)
		{
			if (value <= 240U)
			{
				this.Write((byte)value);
			}
			else if (value <= 2287U)
			{
				this.Write((byte)((value - 240U) / 256U + 241U));
				this.Write((byte)((value - 240U) % 256U));
			}
			else if (value <= 67823U)
			{
				this.Write(249);
				this.Write((byte)((value - 2288U) / 256U));
				this.Write((byte)((value - 2288U) % 256U));
			}
			else if (value <= 16777215U)
			{
				this.Write(250);
				this.Write((byte)(value & 255U));
				this.Write((byte)(value >> 8 & 255U));
				this.Write((byte)(value >> 16 & 255U));
			}
			else
			{
				this.Write(251);
				this.Write((byte)(value & 255U));
				this.Write((byte)(value >> 8 & 255U));
				this.Write((byte)(value >> 16 & 255U));
				this.Write((byte)(value >> 24 & 255U));
			}
		}

		public void WritePackedUInt64(ulong value)
		{
			if (value <= 240UL)
			{
				this.Write((byte)value);
			}
			else if (value <= 2287UL)
			{
				this.Write((byte)((value - 240UL) / 256UL + 241UL));
				this.Write((byte)((value - 240UL) % 256UL));
			}
			else if (value <= 67823UL)
			{
				this.Write(249);
				this.Write((byte)((value - 2288UL) / 256UL));
				this.Write((byte)((value - 2288UL) % 256UL));
			}
			else if (value <= 16777215UL)
			{
				this.Write(250);
				this.Write((byte)(value & 255UL));
				this.Write((byte)(value >> 8 & 255UL));
				this.Write((byte)(value >> 16 & 255UL));
			}
			else if (value <= uint.MaxValue)
			{
				this.Write(251);
				this.Write((byte)(value & 255UL));
				this.Write((byte)(value >> 8 & 255UL));
				this.Write((byte)(value >> 16 & 255UL));
				this.Write((byte)(value >> 24 & 255UL));
			}
			else if (value <= 1099511627775UL)
			{
				this.Write(252);
				this.Write((byte)(value & 255UL));
				this.Write((byte)(value >> 8 & 255UL));
				this.Write((byte)(value >> 16 & 255UL));
				this.Write((byte)(value >> 24 & 255UL));
				this.Write((byte)(value >> 32 & 255UL));
			}
			else if (value <= 281474976710655UL)
			{
				this.Write(253);
				this.Write((byte)(value & 255UL));
				this.Write((byte)(value >> 8 & 255UL));
				this.Write((byte)(value >> 16 & 255UL));
				this.Write((byte)(value >> 24 & 255UL));
				this.Write((byte)(value >> 32 & 255UL));
				this.Write((byte)(value >> 40 & 255UL));
			}
			else if (value <= 72057594037927935UL)
			{
				this.Write(254);
				this.Write((byte)(value & 255UL));
				this.Write((byte)(value >> 8 & 255UL));
				this.Write((byte)(value >> 16 & 255UL));
				this.Write((byte)(value >> 24 & 255UL));
				this.Write((byte)(value >> 32 & 255UL));
				this.Write((byte)(value >> 40 & 255UL));
				this.Write((byte)(value >> 48 & 255UL));
			}
			else
			{
				this.Write(byte.MaxValue);
				this.Write((byte)(value & 255UL));
				this.Write((byte)(value >> 8 & 255UL));
				this.Write((byte)(value >> 16 & 255UL));
				this.Write((byte)(value >> 24 & 255UL));
				this.Write((byte)(value >> 32 & 255UL));
				this.Write((byte)(value >> 40 & 255UL));
				this.Write((byte)(value >> 48 & 255UL));
				this.Write((byte)(value >> 56 & 255UL));
			}
		}

		public void Write(NetworkInstanceId value)
		{
			this.WritePackedUInt32(value.Value);
		}

		public void Write(NetworkSceneId value)
		{
			this.WritePackedUInt32(value.Value);
		}

		public void Write(char value)
		{
			this.m_Buffer.WriteByte((byte)value);
		}

		public void Write(byte value)
		{
			this.m_Buffer.WriteByte(value);
		}

		public void Write(sbyte value)
		{
			this.m_Buffer.WriteByte((byte)value);
		}

		public void Write(short value)
		{
			this.m_Buffer.WriteByte2((byte)(value & 255), (byte)(value >> 8 & 255));
		}

		public void Write(ushort value)
		{
			this.m_Buffer.WriteByte2((byte)(value & 255), (byte)(value >> 8 & 255));
		}

		public void Write(int value)
		{
			this.m_Buffer.WriteByte4((byte)(value & 255), (byte)(value >> 8 & 255), (byte)(value >> 16 & 255), (byte)(value >> 24 & 255));
		}

		public void Write(uint value)
		{
			this.m_Buffer.WriteByte4((byte)(value & 255U), (byte)(value >> 8 & 255U), (byte)(value >> 16 & 255U), (byte)(value >> 24 & 255U));
		}

		public void Write(long value)
		{
			this.m_Buffer.WriteByte8((byte)(value & 255L), (byte)(value >> 8 & 255L), (byte)(value >> 16 & 255L), (byte)(value >> 24 & 255L), (byte)(value >> 32 & 255L), (byte)(value >> 40 & 255L), (byte)(value >> 48 & 255L), (byte)(value >> 56 & 255L));
		}

		public void Write(ulong value)
		{
			this.m_Buffer.WriteByte8((byte)(value & 255UL), (byte)(value >> 8 & 255UL), (byte)(value >> 16 & 255UL), (byte)(value >> 24 & 255UL), (byte)(value >> 32 & 255UL), (byte)(value >> 40 & 255UL), (byte)(value >> 48 & 255UL), (byte)(value >> 56 & 255UL));
		}

		public void Write(float value)
		{
			s_FloatConverter.floatValue = value;
			this.Write(s_FloatConverter.intValue);
		}

		public void Write(double value)
		{
			s_FloatConverter.doubleValue = value;
			this.Write(s_FloatConverter.longValue);
		}

		public void Write(decimal value)
		{
			int[] bits = decimal.GetBits(value);
			this.Write(bits[0]);
			this.Write(bits[1]);
			this.Write(bits[2]);
			this.Write(bits[3]);
		}

		public void Write(string value)
		{
			if (value == null)
			{
				this.m_Buffer.WriteByte2(0, 0);
			}
			else
			{
				int byteCount = s_Encoding.GetByteCount(value);
				if (byteCount >= 32768)
				{
					throw new IndexOutOfRangeException("Serialize(string) too long: " + value.Length);
				}
				this.Write((ushort)byteCount);
				int bytes = s_Encoding.GetBytes(value, 0, value.Length, s_StringWriteBuffer, 0);
				this.m_Buffer.WriteBytes(s_StringWriteBuffer, (ushort)bytes);
			}
		}

		public void Write(bool value)
		{
			if (value)
			{
				this.m_Buffer.WriteByte(1);
			}
			else
			{
				this.m_Buffer.WriteByte(0);
			}
		}

		public void Write(byte[] buffer, int count)
		{
			if (count > 65535)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkWriter Write: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
				}
			}
			else
			{
				this.m_Buffer.WriteBytes(buffer, (ushort)count);
			}
		}

		public void Write(byte[] buffer, int offset, int count)
		{
			if (count > 65535)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkWriter Write: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
				}
			}
			else
			{
				this.m_Buffer.WriteBytesAtOffset(buffer, (ushort)offset, (ushort)count);
			}
		}

		public void WriteBytesAndSize(byte[] buffer, int count)
		{
			if (buffer == null || count == 0)
			{
				this.Write(0);
			}
			else if (count > 65535)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkWriter WriteBytesAndSize: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
				}
			}
			else
			{
				this.Write((ushort)count);
				this.m_Buffer.WriteBytes(buffer, (ushort)count);
			}
		}

		public void WriteBytesFull(byte[] buffer)
		{
			if (buffer == null)
			{
				this.Write(0);
			}
			else if (buffer.Length > 65535)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("NetworkWriter WriteBytes: buffer is too large (" + buffer.Length + ") bytes. The maximum buffer size is 64K bytes.");
				}
			}
			else
			{
				this.Write((ushort)buffer.Length);
				this.m_Buffer.WriteBytes(buffer, (ushort)buffer.Length);
			}
		}

		public void Write(Vector2 value)
		{
			this.Write(value.x);
			this.Write(value.y);
		}

		public void Write(Vector3 value)
		{
			this.Write(value.x);
			this.Write(value.y);
			this.Write(value.z);
		}

		public void Write(Vector4 value)
		{
			this.Write(value.x);
			this.Write(value.y);
			this.Write(value.z);
			this.Write(value.w);
		}

		public void Write(Color value)
		{
			this.Write(value.r);
			this.Write(value.g);
			this.Write(value.b);
			this.Write(value.a);
		}

		public void Write(Color32 value)
		{
			this.Write(value.r);
			this.Write(value.g);
			this.Write(value.b);
			this.Write(value.a);
		}

		public void Write(Quaternion value)
		{
			this.Write(value.x);
			this.Write(value.y);
			this.Write(value.z);
			this.Write(value.w);
		}

		public void Write(Rect value)
		{
			this.Write(value.xMin);
			this.Write(value.yMin);
			this.Write(value.width);
			this.Write(value.height);
		}

		public void Write(Plane value)
		{
			this.Write(value.normal);
			this.Write(value.distance);
		}

		public void Write(Ray value)
		{
			this.Write(value.direction);
			this.Write(value.origin);
		}

		public void Write(Matrix4x4 value)
		{
			this.Write(value.m00);
			this.Write(value.m01);
			this.Write(value.m02);
			this.Write(value.m03);
			this.Write(value.m10);
			this.Write(value.m11);
			this.Write(value.m12);
			this.Write(value.m13);
			this.Write(value.m20);
			this.Write(value.m21);
			this.Write(value.m22);
			this.Write(value.m23);
			this.Write(value.m30);
			this.Write(value.m31);
			this.Write(value.m32);
			this.Write(value.m33);
		}

		public void Write(NetworkHash128 value)
		{
			this.Write(value.i0);
			this.Write(value.i1);
			this.Write(value.i2);
			this.Write(value.i3);
			this.Write(value.i4);
			this.Write(value.i5);
			this.Write(value.i6);
			this.Write(value.i7);
			this.Write(value.i8);
			this.Write(value.i9);
			this.Write(value.i10);
			this.Write(value.i11);
			this.Write(value.i12);
			this.Write(value.i13);
			this.Write(value.i14);
			this.Write(value.i15);
		}

		public void Write(NetworkIdentity value)
		{
			if (value == null)
			{
				this.WritePackedUInt32(0U);
			}
			else
			{
				this.Write(value.netId);
			}
		}

		public void Write(Transform value)
		{
			if (value == null || value.gameObject == null)
			{
				this.WritePackedUInt32(0U);
			}
			else
			{
				NetworkIdentity component = value.gameObject.GetComponent<NetworkIdentity>();
				if (component != null)
				{
					this.Write(component.netId);
				}
				else
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("NetworkWriter " + value + " has no NetworkIdentity");
					}
					this.WritePackedUInt32(0U);
				}
			}
		}

		public void Write(GameObject value)
		{
			if (value == null)
			{
				this.WritePackedUInt32(0U);
			}
			else
			{
				QSBNetworkIdentity component = value.GetComponent<QSBNetworkIdentity>();
				if (component != null)
				{
					this.Write(component.NetId);
				}
				else
				{
					if (LogFilter.logWarn)
					{
						Debug.LogWarning("NetworkWriter " + value + " has no NetworkIdentity");
					}
					this.WritePackedUInt32(0U);
				}
			}
		}

		public void Write(QSBMessageBase msg)
		{
			msg.Serialize(this);
		}

		public void SeekZero()
		{
			this.m_Buffer.SeekZero();
		}

		public void StartMessage(short msgType)
		{
			this.SeekZero();
			this.m_Buffer.WriteByte2(0, 0);
			this.Write(msgType);
		}

		public void FinishMessage()
		{
			this.m_Buffer.FinishMessage();
		}

		private const int k_MaxStringLength = 32768;

		private QSBNetBuffer m_Buffer;

		private static Encoding s_Encoding;

		private static byte[] s_StringWriteBuffer;

		private static QSBUIntFloat s_FloatConverter;
	}
}
