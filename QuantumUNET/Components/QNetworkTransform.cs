using QuantumUNET.Logging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET.Components
{
	public class QNetworkTransform : QNetworkBehaviour
	{
		public float SendInterval { get; set; } = 0.05f;

		private float _lastClientSendTime;
		protected Vector3 _prevPosition;
		protected Quaternion _prevRotation;
		private QNetworkWriter _localTransformWriter;

		public void Awake()
		{
			_prevPosition = transform.position;
			_prevRotation = transform.rotation;
			if (LocalPlayerAuthority)
			{
				_localTransformWriter = new QNetworkWriter();
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

			SerializeTransform(writer, initialState);
			return true;
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

				DeserializeTransform(reader, initialState);
			}
		}

		public virtual void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			writer.Write(transform.position);
			SerializeRotation(writer, transform.rotation);
			_prevPosition = transform.position;
			_prevRotation = transform.rotation;
		}

		public virtual void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			if (HasAuthority)
			{
				reader.ReadVector3();
				DeserializeRotation(reader);
				return;
			}

			transform.position = reader.ReadVector3();
			transform.rotation = DeserializeRotation(reader);
		}

		private void FixedUpdate()
		{
			if (!IsServer)
			{
				return;
			}

			if (SyncVarDirtyBits != 0U)
			{
				return;
			}

			if (!QNetworkServer.active)
			{
				return;
			}

			if (GetNetworkSendInterval() != 0f && HasMoved())
			{
				SetDirtyBit(1U);
			}
		}

		public virtual void Update()
		{
			if (!HasAuthority || !LocalPlayerAuthority)
			{
				return;
			}

			if (QNetworkServer.active)
			{
				return;
			}

			if (Time.time - _lastClientSendTime > GetNetworkSendInterval())
			{
				SendTransform();
				_lastClientSendTime = Time.time;
			}
		}

		public virtual bool HasMoved()
		{
			var displacementMagnitude = (transform.position - _prevPosition).magnitude;
			return displacementMagnitude > 1E-05f
				|| Quaternion.Angle(transform.rotation, _prevRotation) > 1E-05f;
		}

		[Client]
		private void SendTransform()
		{
			if (HasMoved() && QClientScene.readyConnection != null)
			{
				_localTransformWriter.StartMessage(QMsgType.LocalPlayerTransform);
				_localTransformWriter.Write(NetId);
				SerializeTransform(_localTransformWriter, false);
				_prevPosition = transform.position;
				_prevRotation = transform.rotation;
				_localTransformWriter.FinishMessage();

				QClientScene.readyConnection.SendWriter(_localTransformWriter, GetNetworkChannel());
			}
		}

		public static void HandleTransform(QNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var gameObject = QNetworkServer.FindLocalObject(networkInstanceId);
			if (gameObject == null)
			{
				QLog.Warning("Received NetworkTransform data for GameObject that doesn't exist");
				return;
			}

			var component = gameObject.GetComponent<QNetworkTransform>();
			if (component == null)
			{
				QLog.Warning("HandleTransform null target");
				return;
			}

			if (!component.LocalPlayerAuthority)
			{
				QLog.Warning("HandleTransform no localPlayerAuthority");
				return;
			}

			if (netMsg.Connection.ClientOwnedObjects == null)
			{
				QLog.Warning("HandleTransform object not owned by connection");
				return;
			}

			if (netMsg.Connection.ClientOwnedObjects.Contains(networkInstanceId))
			{
				component.DeserializeTransform(netMsg.Reader, false);
			}
			else
			{
				QLog.Warning(
					$"HandleTransform netId:{networkInstanceId} is not for a valid player");
			}
		}

		public static void SerializeRotation(QNetworkWriter writer, Quaternion rot)
		{
			writer.Write(rot.eulerAngles.x);
			writer.Write(rot.eulerAngles.y);
			writer.Write(rot.eulerAngles.z);
		}

		public static Quaternion DeserializeRotation(QNetworkReader reader)
		{
			var rotation = Quaternion.identity;
			var eulerAngles = Vector3.zero;
			eulerAngles.Set(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			rotation.eulerAngles = eulerAngles;
			return rotation;
		}

		public override int GetNetworkChannel()
			=> 1;

		public override float GetNetworkSendInterval()
			=> SendInterval;
	}
}
