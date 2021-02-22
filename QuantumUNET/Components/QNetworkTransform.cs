using QuantumUNET.Logging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET.Components
{
	public class QNetworkTransform : QNetworkBehaviour
	{
		private float m_LastClientSendTime;
		private Vector3 m_PrevPosition;
		private Quaternion m_PrevRotation;
		private QNetworkWriter m_LocalTransformWriter;
		public delegate bool ClientMoveCallback3D(ref Vector3 position, ref Vector3 velocity, ref Quaternion rotation);

		public float SendInterval { get; set; } = 0.1f;
		public bool SyncRotation { get; set; } = true;
		public ClientMoveCallback3D clientMoveCallback3D { get; set; }
		public float LastSyncTime { get; private set; }

		public override int GetNetworkChannel()
			=> 1;

		public override float GetNetworkSendInterval()
			=> SendInterval;

		public override void OnStartAuthority()
			=> LastSyncTime = 0f;

		public void Awake()
		{
			m_PrevPosition = transform.position;
			m_PrevRotation = transform.rotation;
			if (LocalPlayerAuthority)
			{
				m_LocalTransformWriter = new QNetworkWriter();
			}
		}

		public override void OnStartServer()
			=> LastSyncTime = 0f;

		public override bool OnSerialize(QNetworkWriter writer, bool initialState)
		{
			if (!initialState)
			{
				if (SyncVarDirtyBits == 0U)
				{
					writer.WritePackedUInt32(0U);
					return false;
				}
				writer.WritePackedUInt32(1U);
			}
			SerializeModeTransform(writer);
			return true;
		}

		private void SerializeModeTransform(QNetworkWriter writer)
		{
			writer.Write(transform.position);
			if (SyncRotation)
			{
				SerializeRotation3D(writer, transform.rotation);
			}
			m_PrevPosition = transform.position;
			m_PrevRotation = transform.rotation;
		}

		public override void OnDeserialize(QNetworkReader reader, bool initialState)
		{
			if (!IsServer || !QNetworkServer.localClientActive)
			{
				if (!initialState)
				{
					if (reader.ReadPackedUInt32() == 0U)
					{
						return;
					}
				}
				UnserializeModeTransform(reader, initialState);
				LastSyncTime = Time.time;
			}
		}

		private void UnserializeModeTransform(QNetworkReader reader, bool initialState)
		{
			if (HasAuthority)
			{
				reader.ReadVector3();
				if (SyncRotation)
				{
					UnserializeRotation3D(reader);
				}
			}
			else if (IsServer && clientMoveCallback3D != null)
			{
				var position = reader.ReadVector3();
				var zero = Vector3.zero;
				var rotation = Quaternion.identity;
				if (SyncRotation)
				{
					rotation = UnserializeRotation3D(reader);
				}
				if (clientMoveCallback3D(ref position, ref zero, ref rotation))
				{
					transform.position = position;
					if (SyncRotation)
					{
						transform.rotation = rotation;
					}
				}
			}
			else
			{
				transform.position = reader.ReadVector3();
				if (SyncRotation)
				{
					transform.rotation = UnserializeRotation3D(reader);
				}
			}
		}

		private void FixedUpdate()
		{
			if (IsServer)
			{
				FixedUpdateServer();
			}
		}

		private void FixedUpdateServer()
		{
			if (SyncVarDirtyBits == 0U)
			{
				if (QNetworkServer.active)
				{
					if (IsServer)
					{
						if (GetNetworkSendInterval() != 0f)
						{
							if (HasMoved())
							{
								SetDirtyBit(1U);
							}
						}
					}
				}
			}
		}

		private void Update()
		{
			if (HasAuthority)
			{
				if (LocalPlayerAuthority)
				{
					if (!QNetworkServer.active)
					{
						if (Time.time - m_LastClientSendTime > GetNetworkSendInterval())
						{
							SendTransform();
							m_LastClientSendTime = Time.time;
						}
					}
				}
			}
		}

		private bool HasMoved()
		{
			var num = (transform.position - m_PrevPosition).magnitude;
			bool result;
			if (num > 1E-05f)
			{
				result = true;
			}
			else
			{
				num = Quaternion.Angle(transform.rotation, m_PrevRotation);
				if (num > 1E-05f)
				{
					result = true;
				}
				else
				{
					result = num > 1E-05f;
				}
			}
			return result;
		}

		[Client]
		private void SendTransform()
		{
			if (HasMoved() && QClientScene.readyConnection != null)
			{
				m_LocalTransformWriter.StartMessage(6);
				m_LocalTransformWriter.Write(NetId);
				SerializeModeTransform(m_LocalTransformWriter);
				m_PrevPosition = transform.position;
				m_PrevRotation = transform.rotation;
				m_LocalTransformWriter.FinishMessage();
				QClientScene.readyConnection.SendWriter(m_LocalTransformWriter, GetNetworkChannel());
			}
		}

		public static void HandleTransform(QNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var gameObject = QNetworkServer.FindLocalObject(networkInstanceId);
			if (gameObject == null)
			{
				QLog.Warning("Received NetworkTransform data for GameObject that doesn't exist");
			}
			else
			{
				var component = gameObject.GetComponent<QNetworkTransform>();
				if (component == null)
				{
					QLog.Warning("HandleTransform null target");
				}
				else if (!component.LocalPlayerAuthority)
				{
					QLog.Warning("HandleTransform no localPlayerAuthority");
				}
				else if (netMsg.Connection.ClientOwnedObjects == null)
				{
					QLog.Warning("HandleTransform object not owned by connection");
				}
				else if (netMsg.Connection.ClientOwnedObjects.Contains(networkInstanceId))
				{
					component.UnserializeModeTransform(netMsg.Reader, false);
					component.LastSyncTime = Time.time;
				}
				else
				{
					QLog.Warning(
						$"HandleTransform netId:{networkInstanceId} is not for a valid player");
				}
			}
		}

		private static void WriteAngle(QNetworkWriter writer, float angle)
			=> writer.Write(angle);

		private static float ReadAngle(QNetworkReader reader)
			=> reader.ReadSingle();

		public static void SerializeRotation3D(QNetworkWriter writer, Quaternion rot)
		{
			WriteAngle(writer, rot.eulerAngles.x);
			WriteAngle(writer, rot.eulerAngles.y);
			WriteAngle(writer, rot.eulerAngles.z);
		}

		public static Quaternion UnserializeRotation3D(QNetworkReader reader)
		{
			var identity = Quaternion.identity;
			var zero = Vector3.zero;
			zero.Set(ReadAngle(reader), ReadAngle(reader), ReadAngle(reader));
			identity.eulerAngles = zero;
			return identity;
		}
	}
}