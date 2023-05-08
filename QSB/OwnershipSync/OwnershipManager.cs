using Mirror;
using QSB.Messaging;
using QSB.Utility;
using System.Collections.Generic;

namespace QSB.OwnershipSync;

public static class OwnershipManager
{
	#region host only

	/// <summary>
	/// whoever is first gets ownership
	/// </summary>
	private static readonly Dictionary<NetworkIdentity, List<uint>> _ownQueue = new();

	public static void RegisterOwnQueue(this NetworkIdentity identity) => _ownQueue.Add(identity, new List<uint>());
	public static void UnregisterOwnQueue(this NetworkIdentity identity) => _ownQueue.Remove(identity);

	public static void ServerUpdateOwnQueue(this NetworkIdentity identity, uint id, OwnQueueAction action)
	{
		var ownQueue = _ownQueue[identity];
		var oldOwner = ownQueue.Count != 0 ? ownQueue[0] : uint.MaxValue;

		switch (action)
		{
			case OwnQueueAction.Add:
				ownQueue.SafeAdd(id);
				break;

			case OwnQueueAction.Remove:
				ownQueue.Remove(id);
				break;

			case OwnQueueAction.Force:
				ownQueue.Remove(id);
				ownQueue.Insert(0, id);
				break;
		}

		var newOwner = ownQueue.Count != 0 ? ownQueue[0] : uint.MaxValue;
		if (oldOwner != newOwner)
		{
			SetOwner(identity, newOwner);
		}
	}

	/// <summary>
	/// transfer ownership to a different client
	/// </summary>
	public static void OnDisconnect(NetworkConnectionToClient conn)
	{
		var id = conn.GetPlayerId();
		foreach (var identity in _ownQueue.Keys)
		{
			identity.ServerUpdateOwnQueue(id, OwnQueueAction.Remove);
		}
	}

	public static void SetOwner(this NetworkIdentity identity, uint id)
	{
		var oldConn = identity.connectionToClient;
		var newConn = id != uint.MaxValue ? id.GetNetworkConnection() : null;

		if (oldConn == newConn)
		{
			return;
		}

		identity.RemoveClientAuthority();

		if (newConn != null)
		{
			identity.AssignClientAuthority(newConn);
		}
	}

	#endregion

	#region any client

	public static void UpdateOwnQueue(this NetworkIdentity identity, OwnQueueAction action)
	{
		if (action == OwnQueueAction.Force && identity.isOwned)
		{
			return;
		}

		new OwnQueueMessage(identity.netId, action).Send();
	}

	#endregion
}
