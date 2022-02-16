using Mirror;
using System;

namespace QSB.Utility
{
	public abstract class QSBNetworkBehaviour : NetworkBehaviour
	{
		protected virtual float SendInterval => 0.1f;
		protected virtual bool UseReliableRpc => false;

		private double _lastSendTime;

		protected abstract bool HasChanged();
		protected abstract void UpdatePrevData();
		protected abstract void Serialize(NetworkWriter writer);
		protected abstract void Deserialize(NetworkReader reader);

		protected virtual void Update()
		{
			if (!hasAuthority)
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

		public void SendInitialState(uint to)
		{
			if (!hasAuthority)
			{
				return;
			}

			_lastSendTime = NetworkTime.localTime;

			using var writer = NetworkWriterPool.GetWriter();
			Serialize(writer);
			UpdatePrevData();

			var data = writer.ToArraySegment();
			CmdSendInitialData(to, data);
		}

		[Command(channel = Channels.Reliable, requiresAuthority = true)]
		private void CmdSendDataReliable(ArraySegment<byte> data) => RpcSendDataReliable(data);

		[ClientRpc(channel = Channels.Reliable, includeOwner = false)]
		private void RpcSendDataReliable(ArraySegment<byte> data) => OnData(data);

		[Command(channel = Channels.Unreliable, requiresAuthority = true)]
		private void CmdSendDataUnreliable(ArraySegment<byte> data) => RpcSendDataUnreliable(data);

		[ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
		private void RpcSendDataUnreliable(ArraySegment<byte> data) => OnData(data);

		[Command(channel = Channels.Reliable, requiresAuthority = true)]
		private void CmdSendInitialData(uint to, ArraySegment<byte> data) => TargetSendInitialData(to.GetNetworkConnection(), data);

		[TargetRpc(channel = Channels.Reliable)]
		private void TargetSendInitialData(NetworkConnection target, ArraySegment<byte> data) => OnData(data);

		private void OnData(ArraySegment<byte> data)
		{
			using var reader = NetworkReaderPool.GetReader(data);
			UpdatePrevData();
			Deserialize(reader);
		}
	}
}
