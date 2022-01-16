using Mirror;
using System;

namespace QSB.Utility
{
	public abstract class QSBNetworkBehaviour : NetworkBehaviour
	{
		protected virtual float SendInterval => 0.1f;
		protected virtual bool UseReliableRpc => false;

		private double _lastSendTime;

		protected virtual void SerializeInitial(NetworkWriter writer) { }
		protected virtual void DeserializeInitial(NetworkReader reader) { }

		public sealed override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			var changed = base.OnSerialize(writer, initialState);
			if (initialState && isServer)
			{
				SerializeInitial(writer);
			}

			return changed;
		}

		public sealed override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			base.OnDeserialize(reader, initialState);
			if (initialState && !isServer)
			{
				DeserializeInitial(reader);
			}
		}

		protected abstract bool HasChanged();
		protected abstract void UpdatePrevData();
		protected abstract void Serialize(NetworkWriter writer);
		protected abstract void Deserialize(NetworkReader reader);

		protected virtual void Update()
		{
			if (!isClient)
			{
				return;
			}

			if (!hasAuthority)
			{
				return;
			}

			if (!NetworkClient.ready)
			{
				return;
			}

			if (NetworkTime.localTime >= _lastSendTime + SendInterval)
			{
				_lastSendTime = NetworkTime.localTime;

				if (!HasChanged())
				{
					return;
				}

				using var writer = NetworkWriterPool.GetWriter();
				Serialize(writer);
				UpdatePrevData();

				var data = writer.ToArraySegment();
				if (UseReliableRpc)
				{
					CmdSendDataReliable(data);
				}
				else
				{
					CmdSendDataUnreliable(data);
				}
			}
		}

		[Command(channel = Channels.Reliable, requiresAuthority = true)]
		private void CmdSendDataReliable(ArraySegment<byte> data) => RpcSendDataReliable(data);

		[ClientRpc(channel = Channels.Reliable, includeOwner = false)]
		private void RpcSendDataReliable(ArraySegment<byte> data) => OnData(data);

		[Command(channel = Channels.Unreliable, requiresAuthority = true)]
		private void CmdSendDataUnreliable(ArraySegment<byte> data) => RpcSendDataUnreliable(data);

		[ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
		private void RpcSendDataUnreliable(ArraySegment<byte> data) => OnData(data);

		private void OnData(ArraySegment<byte> data)
		{
			using var reader = NetworkReaderPool.GetReader(data);
			UpdatePrevData();
			Deserialize(reader);
		}
	}
}
