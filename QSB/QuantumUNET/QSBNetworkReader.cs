using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBNetworkReader
	{
		public QSBNetworkReader()
		{
			m_buf = new QSBNetBuffer();
            Initialize();
		}

		public QSBNetworkReader(QSBNetworkWriter writer)
		{
			m_buf = new QSBNetBuffer(writer.AsArray());
            Initialize();
		}

		public QSBNetworkReader(byte[] buffer)
		{
			m_buf = new QSBNetBuffer(buffer);
            Initialize();
		}

		private static void Initialize()
		{
			if (s_Encoding == null)
			{
                s_StringReaderBuffer = new byte[1024];
                s_Encoding = new UTF8Encoding();
			}
		}

		public uint Position
		{
			get
			{
				return m_buf.Position;
			}
		}

		public int Length
		{
			get
			{
				return m_buf.Length;
			}
		}

		public void SeekZero()
		{
			m_buf.SeekZero();
		}

		internal void Replace(byte[] buffer)
		{
			m_buf.Replace(buffer);
		}

		public uint ReadPackedUInt32()
		{
			byte b = ReadByte();
			uint result;
			if (b < 241)
			{
				result = (uint)b;
			}
			else
			{
				byte b2 = ReadByte();
				if (b >= 241 && b <= 248)
				{
					result = 240U + 256U * (uint)(b - 241) + (uint)b2;
				}
				else
				{
					byte b3 = ReadByte();
					if (b == 249)
					{
						result = 2288U + 256U * (uint)b2 + (uint)b3;
					}
					else
					{
						byte b4 = ReadByte();
						if (b == 250)
						{
							result = (uint)((int)b2 + ((int)b3 << 8) + ((int)b4 << 16));
						}
						else
						{
							byte b5 = ReadByte();
							if (b < 251)
							{
								throw new IndexOutOfRangeException("ReadPackedUInt32() failure: " + b);
							}
							result = (uint)((int)b2 + ((int)b3 << 8) + ((int)b4 << 16) + ((int)b5 << 24));
						}
					}
				}
			}
			return result;
		}

		public ulong ReadPackedUInt64()
		{
			byte b = ReadByte();
			ulong result;
			if (b < 241)
			{
				result = (ulong)b;
			}
			else
			{
				byte b2 = ReadByte();
				if (b >= 241 && b <= 248)
				{
					result = 240UL + 256UL * ((ulong)b - 241UL) + (ulong)b2;
				}
				else
				{
					byte b3 = ReadByte();
					if (b == 249)
					{
						result = 2288UL + 256UL * (ulong)b2 + (ulong)b3;
					}
					else
					{
						byte b4 = ReadByte();
						if (b == 250)
						{
							result = (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16);
						}
						else
						{
							byte b5 = ReadByte();
							if (b == 251)
							{
								result = (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24);
							}
							else
							{
								byte b6 = ReadByte();
								if (b == 252)
								{
									result = (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32);
								}
								else
								{
									byte b7 = ReadByte();
									if (b == 253)
									{
										result = (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40);
									}
									else
									{
										byte b8 = ReadByte();
										if (b == 254)
										{
											result = (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48);
										}
										else
										{
											byte b9 = ReadByte();
											if (b != 255)
											{
												throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + b);
											}
											result = (ulong)b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48) + ((ulong)b9 << 56);
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		public NetworkInstanceId ReadNetworkId()
		{
			return new NetworkInstanceId(ReadPackedUInt32());
		}

		public NetworkSceneId ReadSceneId()
		{
			return new NetworkSceneId(ReadPackedUInt32());
		}

		public byte ReadByte()
		{
			return m_buf.ReadByte();
		}

		public sbyte ReadSByte()
		{
			return (sbyte)m_buf.ReadByte();
		}

		public short ReadInt16()
		{
			ushort num = 0;
			num |= (ushort)m_buf.ReadByte();
			num |= (ushort)(m_buf.ReadByte() << 8);
			return (short)num;
		}

		public ushort ReadUInt16()
		{
			return (ushort)((uint)(ushort)(0U | (uint)m_buf.ReadByte()) | (uint)(ushort)((uint)m_buf.ReadByte() << 8));
		}

		public int ReadInt32()
		{
			uint num = 0U;
			num |= (uint)m_buf.ReadByte();
			num |= (uint)((uint)m_buf.ReadByte() << 8);
			num |= (uint)((uint)m_buf.ReadByte() << 16);
			return (int)(num | (uint)((uint)m_buf.ReadByte() << 24));
		}

		public uint ReadUInt32()
		{
			uint num = 0U;
			num |= (uint)m_buf.ReadByte();
			num |= (uint)((uint)m_buf.ReadByte() << 8);
			num |= (uint)((uint)m_buf.ReadByte() << 16);
			return num | (uint)((uint)m_buf.ReadByte() << 24);
		}

		public long ReadInt64()
		{
			ulong num = 0UL;
			ulong num2 = (ulong)m_buf.ReadByte();
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 8;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 16;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 24;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 32;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 40;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 48;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 56;
			return (long)(num | num2);
		}

		public ulong ReadUInt64()
		{
			ulong num = 0UL;
			ulong num2 = (ulong)m_buf.ReadByte();
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 8;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 16;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 24;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 32;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 40;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 48;
			num |= num2;
			num2 = (ulong)m_buf.ReadByte() << 56;
			return num | num2;
		}

		public decimal ReadDecimal()
		{
			return new decimal(new int[]
			{
				ReadInt32(),
				ReadInt32(),
				ReadInt32(),
				ReadInt32()
			});
		}

		public float ReadSingle()
		{
			var bytes = ReadBytes(4);
			return BitConverter.ToSingle(bytes, 0);
		}

		public double ReadDouble()
		{
			var bytes = ReadBytes(8);
			return BitConverter.ToSingle(bytes, 0);
		}

		public string ReadString()
		{
			ushort num = ReadUInt16();
			string result;
			if (num == 0)
			{
				result = "";
			}
			else
			{
				if (num >= 32768)
				{
					throw new IndexOutOfRangeException("ReadString() too long: " + num);
				}
				while ((int)num > s_StringReaderBuffer.Length)
				{
                    s_StringReaderBuffer = new byte[s_StringReaderBuffer.Length * 2];
				}
				m_buf.ReadBytes(s_StringReaderBuffer, (uint)num);
				char[] chars = s_Encoding.GetChars(s_StringReaderBuffer, 0, (int)num);
				result = new string(chars);
			}
			return result;
		}

		public char ReadChar()
		{
			return (char)m_buf.ReadByte();
		}

		public bool ReadBoolean()
		{
			int num = (int)m_buf.ReadByte();
			return num == 1;
		}

		public byte[] ReadBytes(int count)
		{
			if (count < 0)
			{
				throw new IndexOutOfRangeException("NetworkReader ReadBytes " + count);
			}
			byte[] array = new byte[count];
			m_buf.ReadBytes(array, (uint)count);
			return array;
		}

		public byte[] ReadBytesAndSize()
		{
			ushort num = ReadUInt16();
			byte[] result;
			if (num == 0)
			{
				result = new byte[0];
			}
			else
			{
				result = ReadBytes((int)num);
			}
			return result;
		}

		public Vector2 ReadVector2()
		{
			return new Vector2(ReadSingle(), ReadSingle());
		}

		public Vector3 ReadVector3()
		{
			return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
		}

		public Vector4 ReadVector4()
		{
			return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
		}

		public Color ReadColor()
		{
			return new Color(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
		}

		public Color32 ReadColor32()
		{
			return new Color32(ReadByte(), ReadByte(), ReadByte(), ReadByte());
		}

		public Quaternion ReadQuaternion()
		{
			return new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
		}

		public Rect ReadRect()
		{
			return new Rect(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
		}

		public Plane ReadPlane()
		{
			return new Plane(ReadVector3(), ReadSingle());
		}

		public Ray ReadRay()
		{
			return new Ray(ReadVector3(), ReadVector3());
		}

		public Matrix4x4 ReadMatrix4x4()
		{
			return new Matrix4x4
			{
				m00 = ReadSingle(),
				m01 = ReadSingle(),
				m02 = ReadSingle(),
				m03 = ReadSingle(),
				m10 = ReadSingle(),
				m11 = ReadSingle(),
				m12 = ReadSingle(),
				m13 = ReadSingle(),
				m20 = ReadSingle(),
				m21 = ReadSingle(),
				m22 = ReadSingle(),
				m23 = ReadSingle(),
				m30 = ReadSingle(),
				m31 = ReadSingle(),
				m32 = ReadSingle(),
				m33 = ReadSingle()
			};
		}

		public NetworkHash128 ReadNetworkHash128()
		{
			NetworkHash128 result;
			result.i0 = ReadByte();
			result.i1 = ReadByte();
			result.i2 = ReadByte();
			result.i3 = ReadByte();
			result.i4 = ReadByte();
			result.i5 = ReadByte();
			result.i6 = ReadByte();
			result.i7 = ReadByte();
			result.i8 = ReadByte();
			result.i9 = ReadByte();
			result.i10 = ReadByte();
			result.i11 = ReadByte();
			result.i12 = ReadByte();
			result.i13 = ReadByte();
			result.i14 = ReadByte();
			result.i15 = ReadByte();
			return result;
		}

		public Transform ReadTransform()
		{
			NetworkInstanceId networkInstanceId = ReadNetworkId();
			Transform result;
			if (networkInstanceId.IsEmpty())
			{
				result = null;
			}
			else
			{
				GameObject gameObject = ClientScene.FindLocalObject(networkInstanceId);
				if (gameObject == null)
				{
					if (LogFilter.logDebug)
					{
						Debug.Log("ReadTransform netId:" + networkInstanceId);
					}
					result = null;
				}
				else
				{
					result = gameObject.transform;
				}
			}
			return result;
		}

		public GameObject ReadGameObject()
		{
			NetworkInstanceId networkInstanceId = ReadNetworkId();
			GameObject result;
			if (networkInstanceId.IsEmpty())
			{
				result = null;
			}
			else
			{
				GameObject gameObject;
				if (QSBNetworkServer.active)
				{
					gameObject = QSBNetworkServer.FindLocalObject(networkInstanceId);
				}
				else
				{
					gameObject = QSBClientScene.FindLocalObject(networkInstanceId);
				}
				if (gameObject == null)
				{
					if (LogFilter.logDebug)
					{
						Debug.Log("ReadGameObject netId:" + networkInstanceId + "go: null");
					}
				}
				result = gameObject;
			}
			return result;
		}

		public QSBNetworkIdentity ReadNetworkIdentity()
		{
			NetworkInstanceId networkInstanceId = ReadNetworkId();
			QSBNetworkIdentity result;
			if (networkInstanceId.IsEmpty())
			{
				result = null;
			}
			else
			{
				GameObject gameObject;
				if (QSBNetworkServer.active)
				{
					gameObject = QSBNetworkServer.FindLocalObject(networkInstanceId);
				}
				else
				{
					gameObject = QSBClientScene.FindLocalObject(networkInstanceId);
				}
				if (gameObject == null)
				{
					if (LogFilter.logDebug)
					{
						Debug.Log("ReadNetworkIdentity netId:" + networkInstanceId + "go: null");
					}
					result = null;
				}
				else
				{
					result = gameObject.GetComponent<QSBNetworkIdentity>();
				}
			}
			return result;
		}

		public override string ToString()
		{
			return m_buf.ToString();
		}

		public TMsg ReadMessage<TMsg>() where TMsg : QSBMessageBase, new()
		{
			TMsg result = Activator.CreateInstance<TMsg>();
			result.Deserialize(this);
			return result;
		}

		private QSBNetBuffer m_buf;

		private const int k_MaxStringLength = 32768;

		private const int k_InitialStringBufferSize = 1024;

		private static byte[] s_StringReaderBuffer;

		private static Encoding s_Encoding;
	}
}