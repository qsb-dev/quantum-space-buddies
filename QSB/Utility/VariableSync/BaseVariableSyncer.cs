using QuantumUNET;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public abstract class BaseVariableSyncer : QNetworkBehaviour
	{
		private float _lastClientSendTime;
		private QNetworkWriter _writer;
		private int _index;
		public bool Ready { get; protected set; }

		public abstract void WriteData(QNetworkWriter writer);
		public abstract void ReadData(QNetworkReader writer);
		public abstract bool HasChanged();

		public virtual void Awake()
		{
			QNetworkServer.RegisterHandlerSafe(short.MaxValue, HandleVariable);

			if (LocalPlayerAuthority)
			{
				_writer = new QNetworkWriter();
			}
		}

		public virtual void Start()
			=> _index = GetComponents<BaseVariableSyncer>().IndexOfReference(this);

		public void OnDestroy()
			=> Ready = false;

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
			if (!IsServer || SyncVarDirtyBits != 0U || !QNetworkServer.active || !Ready)
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
			if (!HasAuthority || !LocalPlayerAuthority || QNetworkServer.active || !Ready)
			{
				return;
			}

			if (Time.time - _lastClientSendTime > GetNetworkSendInterval())
			{
				SendVariable();
				_lastClientSendTime = Time.time;
			}
		}

		private void SendVariable()
		{
			if (HasChanged() && QClientScene.readyConnection != null)
			{
				_writer.StartMessage(short.MaxValue);
				_writer.Write(NetId);
				_writer.Write(_index);
				WriteData(_writer);
				_writer.FinishMessage();
				QClientScene.readyConnection.SendWriter(_writer, GetNetworkChannel());
			}
		}

		public static void HandleVariable(QNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var index = netMsg.Reader.ReadInt32();
			var gameObject = QNetworkServer.FindLocalObject(networkInstanceId);
			var components = gameObject.GetComponents<BaseVariableSyncer>();
			components[index].ReadData(netMsg.Reader);
		}
	}
}
