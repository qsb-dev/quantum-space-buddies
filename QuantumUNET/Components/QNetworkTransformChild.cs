using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET.Components
{
	public class QNetworkTransformChild : QNetworkBehaviour
	{
		public Transform Target
		{
			get => m_Target;
			set => m_Target = value;
		}

		public uint ChildIndex => m_ChildIndex;

		public float SendInterval
		{
			get => m_SendInterval;
			set => m_SendInterval = value;
		}

		public float MovementThreshold
		{
			get => m_MovementThreshold;
			set => m_MovementThreshold = value;
		}

		public float InterpolateRotation
		{
			get => m_InterpolateRotation;
			set => m_InterpolateRotation = value;
		}

		public float InterpolateMovement
		{
			get => m_InterpolateMovement;
			set => m_InterpolateMovement = value;
		}

		public float LastSyncTime { get; private set; }
		public Vector3 TargetSyncPosition => m_TargetSyncPosition;
		public Quaternion TargetSyncRotation3D => m_TargetSyncRotation3D;

		private void Awake()
		{
			m_PrevPosition = m_Target.localPosition;
			m_PrevRotation = m_Target.localRotation;
			if (LocalPlayerAuthority)
			{
				m_LocalTransformWriter = new QNetworkWriter();
			}
		}

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
			writer.Write(m_Target.localPosition);
			QNetworkTransform.SerializeRotation(writer, m_Target.localRotation);
			m_PrevPosition = m_Target.localPosition;
			m_PrevRotation = m_Target.localRotation;
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

		// Token: 0x06000488 RID: 1160 RVA: 0x00019B44 File Offset: 0x00017D44
		private void UnserializeModeTransform(QNetworkReader reader, bool initialState)
		{
			if (HasAuthority)
			{
				reader.ReadVector3();
				QNetworkTransform.DeserializeRotation(reader);
			}
			else
			{
				m_TargetSyncPosition = reader.ReadVector3();
				m_TargetSyncRotation3D = QNetworkTransform.DeserializeRotation(reader);
			}
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x00019C4B File Offset: 0x00017E4B
		private void FixedUpdate()
		{
			if (IsServer)
			{
				FixedUpdateServer();
			}
			if (IsClient)
			{
				FixedUpdateClient();
			}
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x00019C74 File Offset: 0x00017E74
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
							var num = (m_Target.localPosition - m_PrevPosition).sqrMagnitude;
							if (num < MovementThreshold)
							{
								num = Quaternion.Angle(m_PrevRotation, m_Target.localRotation);
								if (num < MovementThreshold)
								{
									return;
								}
							}
							SetDirtyBit(1U);
						}
					}
				}
			}
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x00019D24 File Offset: 0x00017F24
		private void FixedUpdateClient()
		{
			if (LastSyncTime != 0f)
			{
				if (QNetworkServer.active || QNetworkClient.active)
				{
					if (IsServer || IsClient)
					{
						if (GetNetworkSendInterval() != 0f)
						{
							if (!HasAuthority)
							{
								if (LastSyncTime != 0f)
								{
									m_Target.localPosition = m_InterpolateMovement > 0f
										? Vector3.Lerp(m_Target.localPosition, m_TargetSyncPosition, m_InterpolateMovement)
										: m_TargetSyncPosition;
									m_Target.localRotation = m_InterpolateRotation > 0f
										? Quaternion.Slerp(m_Target.localRotation, m_TargetSyncRotation3D, m_InterpolateRotation)
										: m_TargetSyncRotation3D;
								}
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
			var num = (m_Target.localPosition - m_PrevPosition).sqrMagnitude;
			bool result;
			if (num > 1E-05f)
			{
				result = true;
			}
			else
			{
				num = Quaternion.Angle(m_Target.localRotation, m_PrevRotation);
				result = num > 1E-05f;
			}
			return result;
		}

		[Client]
		private void SendTransform()
		{
			if (HasMoved() && QClientScene.readyConnection != null)
			{
				m_LocalTransformWriter.StartMessage(16);
				m_LocalTransformWriter.Write(NetId);
				m_LocalTransformWriter.WritePackedUInt32(m_ChildIndex);
				SerializeModeTransform(m_LocalTransformWriter);
				m_PrevPosition = m_Target.localPosition;
				m_PrevRotation = m_Target.localRotation;
				m_LocalTransformWriter.FinishMessage();
				QClientScene.readyConnection.SendWriter(m_LocalTransformWriter, GetNetworkChannel());
			}
		}

		internal static void HandleChildTransform(QNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var num = netMsg.Reader.ReadPackedUInt32();
			var gameObject = QNetworkServer.FindLocalObject(networkInstanceId);
			if (gameObject == null)
			{
				Debug.LogError("Received NetworkTransformChild data for GameObject that doesn't exist");
			}
			else
			{
				var components = gameObject.GetComponents<QNetworkTransformChild>();
				if (components == null || components.Length == 0)
				{
					Debug.LogError("HandleChildTransform no children");
				}
				else if (num >= (ulong)components.Length)
				{
					Debug.LogError("HandleChildTransform childIndex invalid");
				}
				else
				{
					var networkTransformChild = components[(int)(UIntPtr)num];
					if (networkTransformChild == null)
					{
						Debug.LogError("HandleChildTransform null target");
					}
					else if (!networkTransformChild.LocalPlayerAuthority)
					{
						Debug.LogError("HandleChildTransform no localPlayerAuthority");
					}
					else if (!netMsg.Connection.ClientOwnedObjects.Contains(networkInstanceId))
					{
						Debug.LogWarning("NetworkTransformChild netId:" + networkInstanceId + " is not for a valid player");
					}
					else
					{
						networkTransformChild.UnserializeModeTransform(netMsg.Reader, false);
						networkTransformChild.LastSyncTime = Time.time;
						if (!networkTransformChild.IsClient)
						{
							networkTransformChild.m_Target.localPosition = networkTransformChild.m_TargetSyncPosition;
							networkTransformChild.m_Target.localRotation = networkTransformChild.m_TargetSyncRotation3D;
						}
					}
				}
			}
		}

		public override int GetNetworkChannel() => 1;
		public override float GetNetworkSendInterval() => m_SendInterval;
		public Transform m_Target;
		public uint m_ChildIndex;
		public QNetworkTransform m_Root;
		public float m_SendInterval = 0.1f;
		public float m_MovementThreshold = 0.001f;
		public float m_InterpolateRotation = 0.5f;
		public float m_InterpolateMovement = 0.5f;
		private Vector3 m_TargetSyncPosition;
		private Quaternion m_TargetSyncRotation3D;
		private float m_LastClientSendTime;
		private Vector3 m_PrevPosition;
		private Quaternion m_PrevRotation;
		private const float k_LocalMovementThreshold = 1E-05f;
		private const float k_LocalRotationThreshold = 1E-05f;
		private QNetworkWriter m_LocalTransformWriter;
	}
}
