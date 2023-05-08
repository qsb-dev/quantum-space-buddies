using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.OwnershipSync;

/// <summary>
/// always sent to host
/// </summary>
public class OwnerQueueMessage : QSBMessage<(uint NetId, OwnerQueueAction Action)>
{
	public OwnerQueueMessage(uint netId, OwnerQueueAction action) : base((netId, action)) =>
		To = 0;

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;
	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote() => NetworkServer.spawned[Data.NetId].ServerUpdateOwnerQueue(From, Data.Action);
}

public enum OwnerQueueAction
{
	/// <summary>
	/// add player to the queue
	/// </summary>
	Add,
	/// <summary>
	/// remove player from the queue
	/// </summary>
	Remove,
	/// <summary>
	/// add player to the queue and force them to the front
	/// </summary>
	Force
}