using QuantumUNET;
using QuantumUNET.Components;
using QuantumUNET.Logging;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Utility.VariableSync
{
	internal abstract class BaseVariableSyncer : QNetworkBehaviour
	{
		private float _lastClientSendTime;
		private QNetworkWriter _localTransformWriter;

		public void Awake()
		{
			if (LocalPlayerAuthority)
			{
				_localTransformWriter = new QNetworkWriter();
			}
		}

		private void Start()
		{
			DebugLog.DebugWrite($"add handler");
			QNetworkServer.instance.m_SimpleServerSimple.RegisterHandlerSafe((short)QSB.Events.EventType.VariableSync, HandleTransform);
		}

		public override bool OnSerialize(QNetworkWriter writer, bool initialState)
		{
			DebugLog.DebugWrite($"ON SERIALIZE");

			if (!initialState)
			{
				if (SyncVarDirtyBits == 0U)
				{
					writer.WritePackedUInt32(0U);
					return false;
				}

				writer.WritePackedUInt32(1U);
			}

			WriteData(writer);
			return true;
		}

		public override void OnDeserialize(QNetworkReader reader, bool initialState)
		{
			DebugLog.DebugWrite($"ON DESERIALIZE");

			if (!IsServer || !QNetworkServer.localClientActive)
			{
				if (!initialState && reader.ReadPackedUInt32() == 0U)
				{
					return;
				}

				ReadData(reader);
			}
		}

		private void FixedUpdate()
		{
			if (!IsServer)
			{
				DebugLog.DebugWrite($"FIXEDUPDATE not server");
				return;
			}

			if (SyncVarDirtyBits != 0U)
			{
				DebugLog.DebugWrite($"FIXEDUPDATE dirty!!!");
				return;
			}

			if (!QNetworkServer.active)
			{
				DebugLog.DebugWrite($"FIXEDUPDATE server is not active!");
				return;
			}

			if (GetNetworkSendInterval() != 0f && HasChanged())
			{
				SetDirtyBit(1U);
			}
		}

		public virtual void Update()
		{
			if (!HasAuthority || !LocalPlayerAuthority)
			{
				DebugLog.DebugWrite($"UPDATE no authority");
				return;
			}

			if (QNetworkServer.active)
			{
				DebugLog.DebugWrite($"UPDATE server active");
				return;
			}

			if (Time.time - _lastClientSendTime > GetNetworkSendInterval())
			{
				this.SendTransform();
				this._lastClientSendTime = Time.time;
			}
		}

		[Client]
		private void SendTransform()
		{
			DebugLog.DebugWrite($"SEND TRANSFORM");

			if (HasChanged() && QClientScene.readyConnection != null)
			{
				_localTransformWriter.StartMessage(6);
				_localTransformWriter.Write(NetId);
				WriteData(_localTransformWriter);
				_localTransformWriter.FinishMessage();
				QClientScene.readyConnection.SendWriter(_localTransformWriter);
			}
		}

		public static void HandleTransform(QNetworkMessage netMsg)
		{
			DebugLog.DebugWrite($"HANDLE TRANSFORM");

			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var gameObject = QNetworkServer.FindLocalObject(networkInstanceId);
			if (gameObject == null)
			{
				QLog.Warning("Received NetworkTransform data for GameObject that doesn't exist");
				return;
			}

			var component = gameObject.GetComponent<BaseVariableSyncer>();
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
				component.ReadData(netMsg.Reader);
			}
			else
			{
				QLog.Warning(
					$"HandleTransform netId:{networkInstanceId} is not for a valid player");
			}
		}

		public abstract bool HasChanged();
		public abstract void WriteData(QNetworkWriter writer);
		public abstract void ReadData(QNetworkReader writer);
	}
}
