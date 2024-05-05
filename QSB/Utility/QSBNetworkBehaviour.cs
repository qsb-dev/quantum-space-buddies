using Mirror;
using QSB.WorldSync;
using QSB.WorldSync.Messages;
using System;

namespace QSB.Utility;

public abstract class QSBNetworkBehaviour : NetworkBehaviour
{
	protected virtual float SendInterval => 0.1f;
	protected virtual bool UseReliableRpc => false;

	private double _lastSendTime;
	private byte[] _lastKnownData;

	public override void OnStartClient()
	{
		DontDestroyOnLoad(gameObject);
	}

	/// <summary>
	/// checked before serializing
	/// </summary>
	protected abstract bool HasChanged();

	protected abstract void Serialize(NetworkWriter writer);

	/// <summary>
	/// called right after serializing
	/// </summary>
	protected abstract void UpdatePrevData();

	protected abstract void Deserialize(NetworkReader reader);

	public bool IsValid { get; private set; }
	protected virtual bool CheckValid() => QSBWorldSync.AllObjectsReady;

	protected virtual void Update()
	{
		IsValid = CheckValid();
		if (!IsValid)
		{
			return;
		}

		if (!isOwned)
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

			using var writer = NetworkWriterPool.Get();
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

			if (QSBCore.IsHost)
			{
				_lastKnownData ??= new byte[data.Count];
				Array.Copy(data.Array!, data.Offset, _lastKnownData, 0, data.Count);
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
		if (!IsValid)
		{
			return;
		}

		if (isOwned)
		{
			return;
		}

		if (QSBCore.IsHost)
		{
			_lastKnownData ??= new byte[data.Count];
			Array.Copy(data.Array!, data.Offset, _lastKnownData, 0, data.Count);
		}

		using var reader = NetworkReaderPool.Get(data);
		Deserialize(reader);
	}
}
