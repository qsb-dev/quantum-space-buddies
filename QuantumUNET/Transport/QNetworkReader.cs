using QuantumUNET.Components;
using QuantumUNET.Messages;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET.Transport
{
	public class QNetworkReader : BinaryReader
	{
        public QNetworkReader(QNetworkWriter writer) : base(new MemoryStream(writer.ToArray()))
        {
		}
        public QNetworkReader(byte[] buffer) : base(new MemoryStream(buffer))
        {
        }        

		public uint ReadPackedUInt32()
		{
			var b = ReadByte();
			uint result;
			if (b < 241)
			{
				result = b;
			}
			else
			{
				var b2 = ReadByte();
				if (b >= 241 && b <= 248)
				{
					result = 240U + (uint)(b - 241)<<8 + b2;
				}
				else
				{
					var b3 = ReadByte();
					if (b == 249)
					{
						result = 2288U + b2<<8 + b3;
					}
					else
					{
						var b4 = ReadByte();
						if (b == 250)
						{
							result = (uint)(b2 + (b3 << 8) + (b4 << 16));
						}
						else
						{
							var b5 = ReadByte();
							if (b < 251)
							{
								throw new IndexOutOfRangeException($"ReadPackedUInt32() failure: {b}");
							}

							result = (uint)(b2 + (b3 << 8) + (b4 << 16) + (b5 << 24));
						}
					}
				}
			}

			return result;
		}

		public ulong ReadPackedUInt64()
		{
			var b = ReadByte();
			ulong result;
			if (b < 241)
			{
				result = b;
			}
			else
			{
				var b2 = ReadByte();
				if (b >= 241 && b <= 248)
				{
					result = 240UL + (b - 241UL)<<8 + b2;
				}
				else
				{
					var b3 = ReadByte();
					if (b == 249)
					{
						result = 2288UL + b2<<8 + b3;
					}
					else
					{
						var b4 = ReadByte();
						if (b == 250)
						{
							result = b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16);
						}
						else
						{
							var b5 = ReadByte();
							if (b == 251)
							{
								result = b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24);
							}
							else
							{
								var b6 = ReadByte();
								if (b == 252)
								{
									result = b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32);
								}
								else
								{
									var b7 = ReadByte();
									if (b == 253)
									{
										result = b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40);
									}
									else
									{
										var b8 = ReadByte();
										if (b == 254)
										{
											result = b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48);
										}
										else
										{
											var b9 = ReadByte();
											if (b != 255)
											{
												throw new IndexOutOfRangeException($"ReadPackedUInt64() failure: {b}");
											}

											result = b2 + ((ulong)b3 << 8) + ((ulong)b4 << 16) + ((ulong)b5 << 24) + ((ulong)b6 << 32) + ((ulong)b7 << 40) + ((ulong)b8 << 48) + ((ulong)b9 << 56);
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

		public NetworkInstanceId ReadNetworkId() => new NetworkInstanceId(ReadPackedUInt32());

		public NetworkSceneId ReadSceneId() => new NetworkSceneId(ReadPackedUInt32());
        
		public byte[] ReadByteArray()
		{
			var num = ReadUInt16();
			return num == 0 ? new byte[0] : ReadBytes(num);
		}

		public Vector2 ReadVector2() => new Vector2(ReadSingle(), ReadSingle());

		public Vector3 ReadVector3() => new Vector3(ReadSingle(), ReadSingle(), ReadSingle());

		public Vector4 ReadVector4() => new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

		public Color ReadColor() => new Color(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

		public Color32 ReadColor32() => new Color32(ReadByte(), ReadByte(), ReadByte(), ReadByte());

		public Quaternion ReadQuaternion() => new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

		public Rect ReadRect() => new Rect(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

		public Plane ReadPlane() => new Plane(ReadVector3(), ReadSingle());

		public Ray ReadRay() => new Ray(ReadVector3(), ReadVector3());

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
			var networkInstanceId = ReadNetworkId();
			Transform result;
			if (networkInstanceId.IsEmpty())
			{
				result = null;
			}
			else
			{
				var gameObject = QClientScene.FindLocalObject(networkInstanceId);
				if (gameObject == null)
				{
					Debug.Log($"ReadTransform netId:{networkInstanceId}");
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
			var networkInstanceId = ReadNetworkId();
			GameObject result;
			if (networkInstanceId.IsEmpty())
			{
				result = null;
			}
			else
			{
				var gameObject = QNetworkServer.active
					? QNetworkServer.FindLocalObject(networkInstanceId)
					: QClientScene.FindLocalObject(networkInstanceId);
				if (gameObject == null)
				{
					Debug.Log($"ReadGameObject netId:{networkInstanceId}go: null");
				}

				result = gameObject;
			}

			return result;
		}

		public QNetworkIdentity ReadNetworkIdentity()
		{
			var networkInstanceId = ReadNetworkId();
			QNetworkIdentity result;
			if (networkInstanceId.IsEmpty())
			{
				result = null;
			}
			else
			{
				var gameObject = QNetworkServer.active
					? QNetworkServer.FindLocalObject(networkInstanceId)
					: QClientScene.FindLocalObject(networkInstanceId);
				if (gameObject == null)
				{
					Debug.Log($"ReadNetworkIdentity netId:{networkInstanceId}go: null");
					result = null;
				}
				else
				{
					result = gameObject.GetComponent<QNetworkIdentity>();
				}
			}

			return result;
		}
        //Because we don't have access to the buffer this is the best that I could think of - ShoosGun (Locochoco)
		public override string ToString() => BaseStream.ToString();

		public TMsg ReadMessage<TMsg>() where TMsg : QMessageBase, new()
		{
			var result = Activator.CreateInstance<TMsg>();
			result.Deserialize(this);
			return result;
		}
	}
}