using QuantumUNET;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Utility.VariableSync
{
	public abstract class BaseVariableSyncer : QNetworkBehaviour
	{
		private float _lastClientSendTime;
		private QNetworkWriter _writer;

		public abstract bool HasChanged();
		public abstract void WriteData(QNetworkWriter writer);
		public abstract void ReadData(QNetworkReader writer);

		public void Awake()
		{
			QNetworkServer.instance.m_SimpleServerSimple.RegisterHandlerSafe((short)QSB.Events.EventType.VariableSync, HandleVariable);

			if (LocalPlayerAuthority)
			{
				_writer = new QNetworkWriter();
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

			WriteData(writer);
			return true;
		}

		public override void OnDeserialize(QNetworkReader reader, bool initialState)
		{
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
			if (!IsServer || SyncVarDirtyBits != 0U || !QNetworkServer.active)
			{
				return;
			}

			if (GetNetworkSendInterval() != 0f && HasChanged())
			{
				SetDirtyBit(1U);
			}
		}

		public virtual void Update()
		{
			if (!HasAuthority || !LocalPlayerAuthority || QNetworkServer.active)
			{
				return;
			}

			if (Time.time - _lastClientSendTime > GetNetworkSendInterval())
			{
				SendVariable();
				_lastClientSendTime = Time.time;
			}
		}

		[Client]
		private void SendVariable()
		{
			if (HasChanged() && QClientScene.readyConnection != null)
			{
				_writer.StartMessage((short)QSB.Events.EventType.VariableSync);
				_writer.Write(NetId);
				WriteData(_writer);
				_writer.FinishMessage();
				QClientScene.readyConnection.SendWriter(_writer, GetNetworkChannel());
			}
		}

		public static void HandleVariable(QNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var gameObject = QNetworkServer.FindLocalObject(networkInstanceId);
			var component = gameObject.GetComponent<BaseVariableSyncer>();
			component.ReadData(netMsg.Reader);
		}
	}
}
