using QuantumUNET.Components;
using QuantumUNET.Messages;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET.Transport
{
	public class QNetworkWriter : BinaryWriter
	{
        private MemoryStream _memStream;

        public QNetworkWriter() : base()
		{
            _memStream = new MemoryStream();
            OutStream = _memStream;
		}

		public QNetworkWriter(byte[] buffer) : base()
        {
            _memStream = new MemoryStream(buffer);
            OutStream = _memStream;
        }
        public short Position => (short)_memStream.Position;

		public byte[] ToArray()
		{
            byte[] data = _memStream.ToArray();
            return data;
        }

        public byte[] AsArray() => AsArraySegment().Array;

        internal ArraySegment<byte> AsArraySegment() => new ArraySegment<byte>(_memStream.ToArray(), 0, (int)_memStream.Position);

		public void WritePackedUInt32(uint value)
		{
			if (value <= 240U)
			{
				Write((byte)value);
			}
			else if (value <= 2287U)
			{
				Write((byte)((value - 240U) / 256U + 241U));
				Write((byte)((value - 240U) % 256U));
			}
			else if (value <= 67823U)
			{
				Write(249);
				Write((byte)((value - 2288U) / 256U));
				Write((byte)((value - 2288U) % 256U));
			}
			else if (value <= 16777215U)
			{
				Write(250);
				Write((byte)(value & 255U));
				Write((byte)((value >> 8) & 255U));
				Write((byte)((value >> 16) & 255U));
			}
			else
			{
				Write(251);
				Write((byte)(value & 255U));
				Write((byte)((value >> 8) & 255U));
				Write((byte)((value >> 16) & 255U));
				Write((byte)((value >> 24) & 255U));
			}
		}

		public void WritePackedUInt64(ulong value)
		{
			if (value <= 240UL)
			{
				Write((byte)value);
			}
			else if (value <= 2287UL)
			{
				Write((byte)((value - 240UL) / 256UL + 241UL));
				Write((byte)((value - 240UL) % 256UL));
			}
			else if (value <= 67823UL)
			{
				Write(249);
				Write((byte)((value - 2288UL) / 256UL));
				Write((byte)((value - 2288UL) % 256UL));
			}
			else if (value <= 16777215UL)
			{
				Write(250);
				Write((byte)(value & 255UL));
				Write((byte)((value >> 8) & 255UL));
				Write((byte)((value >> 16) & 255UL));
			}
			else if (value <= uint.MaxValue)
			{
				Write(251);
				Write((byte)(value & 255UL));
				Write((byte)((value >> 8) & 255UL));
				Write((byte)((value >> 16) & 255UL));
				Write((byte)((value >> 24) & 255UL));
			}
			else if (value <= 1099511627775UL)
			{
				Write(252);
				Write((byte)(value & 255UL));
				Write((byte)((value >> 8) & 255UL));
				Write((byte)((value >> 16) & 255UL));
				Write((byte)((value >> 24) & 255UL));
				Write((byte)((value >> 32) & 255UL));
			}
			else if (value <= 281474976710655UL)
			{
				Write(253);
				Write((byte)(value & 255UL));
				Write((byte)((value >> 8) & 255UL));
				Write((byte)((value >> 16) & 255UL));
				Write((byte)((value >> 24) & 255UL));
				Write((byte)((value >> 32) & 255UL));
				Write((byte)((value >> 40) & 255UL));
			}
			else if (value <= 72057594037927935UL)
			{
				Write(254);
				Write((byte)(value & 255UL));
				Write((byte)((value >> 8) & 255UL));
				Write((byte)((value >> 16) & 255UL));
				Write((byte)((value >> 24) & 255UL));
				Write((byte)((value >> 32) & 255UL));
				Write((byte)((value >> 40) & 255UL));
				Write((byte)((value >> 48) & 255UL));
			}
			else
			{
				Write(byte.MaxValue);
				Write((byte)(value & 255UL));
				Write((byte)((value >> 8) & 255UL));
				Write((byte)((value >> 16) & 255UL));
				Write((byte)((value >> 24) & 255UL));
				Write((byte)((value >> 32) & 255UL));
				Write((byte)((value >> 40) & 255UL));
				Write((byte)((value >> 48) & 255UL));
				Write((byte)((value >> 56) & 255UL));
			}
		}

		public void Write(NetworkInstanceId value) => WritePackedUInt32(value.Value);

		public void Write(NetworkSceneId value) => WritePackedUInt32(value.Value);		

		public void WriteBytesAndSize(byte[] buffer, int count)
		{
			if (buffer == null || count == 0)
			{
				Write(0);
			}
			else if (count > 65535)
			{
				Debug.LogError($"NetworkWriter WriteBytesAndSize: buffer is too large ({count}) bytes. The maximum buffer size is 64K bytes.");
			}
			else
			{
				Write(buffer, 0, (ushort)count);
			}
		}

		public void WriteBytesFull(byte[] buffer)
		{
			if (buffer == null)
			{
				Write(0);
			}
			else if (buffer.Length > 65535)
			{
				Debug.LogError($"NetworkWriter WriteBytes: buffer is too large ({buffer.Length}) bytes. The maximum buffer size is 64K bytes.");
			}
			else
			{
				Write(buffer);
			}
		}

		public void Write(Vector2 value)
		{
			Write(value.x);
			Write(value.y);
		}

		public void Write(Vector3 value)
		{
			Write(value.x);
			Write(value.y);
			Write(value.z);
		}

		public void Write(Vector4 value)
		{
			Write(value.x);
			Write(value.y);
			Write(value.z);
			Write(value.w);
		}

		public void Write(Color value)
		{
			Write(value.r);
			Write(value.g);
			Write(value.b);
			Write(value.a);
		}

		public void Write(Color32 value)
		{
			Write(value.r);
			Write(value.g);
			Write(value.b);
			Write(value.a);
		}

		public void Write(Quaternion value)
		{
			Write(value.x);
			Write(value.y);
			Write(value.z);
			Write(value.w);
		}

		public void Write(Rect value)
		{
			Write(value.xMin);
			Write(value.yMin);
			Write(value.width);
			Write(value.height);
		}

		public void Write(Plane value)
		{
			Write(value.normal);
			Write(value.distance);
		}

		public void Write(Ray value)
		{
			Write(value.direction);
			Write(value.origin);
		}

		public void Write(Matrix4x4 value)
		{
			Write(value.m00);
			Write(value.m01);
			Write(value.m02);
			Write(value.m03);
			Write(value.m10);
			Write(value.m11);
			Write(value.m12);
			Write(value.m13);
			Write(value.m20);
			Write(value.m21);
			Write(value.m22);
			Write(value.m23);
			Write(value.m30);
			Write(value.m31);
			Write(value.m32);
			Write(value.m33);
		}

		public void Write(NetworkHash128 value)
		{
			Write(value.i0);
			Write(value.i1);
			Write(value.i2);
			Write(value.i3);
			Write(value.i4);
			Write(value.i5);
			Write(value.i6);
			Write(value.i7);
			Write(value.i8);
			Write(value.i9);
			Write(value.i10);
			Write(value.i11);
			Write(value.i12);
			Write(value.i13);
			Write(value.i14);
			Write(value.i15);
		}

		public void Write(QNetworkIdentity value)
		{
			if (value == null)
			{
				WritePackedUInt32(0U);
			}
			else
			{
				Write(value.NetId);
			}
		}

		public void Write(Transform value)
		{
			if (value == null || value.gameObject == null)
			{
				WritePackedUInt32(0U);
			}
			else
			{
				var component = value.gameObject.GetComponent<QNetworkIdentity>();
				if (component != null)
				{
					Write(component.NetId);
				}
				else
				{
					Debug.LogWarning($"NetworkWriter {value} has no NetworkIdentity");
					WritePackedUInt32(0U);
				}
			}
		}

		public void Write(GameObject value)
		{
			if (value == null)
			{
				WritePackedUInt32(0U);
			}
			else
			{
				var component = value.GetComponent<QNetworkIdentity>();
				if (component != null)
				{
					Write(component.NetId);
				}
				else
				{
					Debug.LogWarning($"NetworkWriter {value} has no NetworkIdentity");
					WritePackedUInt32(0U);
				}
			}
		}

		public void Write(QMessageBase msg) => msg.Serialize(this);

		public void SeekZero() => _memStream.Position = 0;

		public void StartMessage(short msgType)
		{
			SeekZero();
            Write(new byte[] { 0, 0 });
			Write(msgType);
		}

        public void FinishMessage()
        {
            var num = (ushort)(_memStream.Position - 4U);
            _memStream.Position = 0;
            Write(num);
        }

		private const int k_MaxStringLength = 32768;
	}
}