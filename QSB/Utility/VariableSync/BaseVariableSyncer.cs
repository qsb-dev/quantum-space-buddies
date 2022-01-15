using QuantumUNET;
using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public abstract class BaseVariableSyncer2 : QSBNetworkBehaviour
	{
		protected override float SendInterval => 0.1f;
	}

	public interface IBaseVariableSyncer
	{
		void DeserializeValue(QNetworkReader reader);
	}

	public abstract class BaseVariableSyncer<T> : QNetworkBehaviour, IBaseVariableSyncer
	{
		private float _lastClientSendTime;
		private QNetworkWriter _writer;
		private int _index;
		private bool _initialized;

		private T _prevValue;
		public T Value { get; set; }

		protected abstract void WriteValue(QNetworkWriter writer, T value);
		protected abstract T ReadValue(QNetworkReader reader);

		private bool HasChanged() => !EqualityComparer<T>.Default.Equals(Value, _prevValue);

		public virtual void Awake()
		{
			QNetworkServer.RegisterHandlerSafe(short.MaxValue, HandleVariable);

			if (LocalPlayerAuthority)
			{
				_writer = new QNetworkWriter();
			}
		}

		public virtual void Start()
			=> _index = GetComponents<IBaseVariableSyncer>().IndexOf(this);

		public void Init() => _initialized = true;
		public void OnDestroy() => _initialized = false;

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

			SerializeValue(writer);
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

				DeserializeValue(reader);
			}
		}

		public void SerializeValue(QNetworkWriter writer)
		{
			if (_initialized)
			{
				WriteValue(writer, Value);
				_prevValue = Value;
			}
			else
			{
				WriteValue(writer, default);
			}
		}

		public void DeserializeValue(QNetworkReader reader)
		{
			if (_initialized)
			{
				Value = ReadValue(reader);
			}
			else
			{
				ReadValue(reader);
			}
		}

		private void FixedUpdate()
		{
			if (!IsServer || SyncVarDirtyBits != 0U || !QNetworkServer.active || !_initialized)
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
			if (!HasAuthority || !LocalPlayerAuthority || QNetworkServer.active || !_initialized)
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
				SerializeValue(_writer);
				_writer.FinishMessage();
				QClientScene.readyConnection.SendWriter(_writer, GetNetworkChannel());
			}
		}

		public static void HandleVariable(QNetworkMessage netMsg)
		{
			var networkInstanceId = netMsg.Reader.ReadNetworkId();
			var gameObject = QNetworkServer.FindLocalObject(networkInstanceId);
			var index = netMsg.Reader.ReadInt32();
			var component = gameObject.GetComponents<IBaseVariableSyncer>()[index];
			component.DeserializeValue(netMsg.Reader);
		}
	}
}
