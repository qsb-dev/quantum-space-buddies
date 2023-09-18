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
	private static readonly Dictionary<NetworkIdentity, List<uint>> _ownerQueue = new();

	public static void RegisterOwnerQueue(this NetworkIdentity identity) => _ownerQueue.Add(identity, new List<uint>());
	public static void UnregisterOwnerQueue(this NetworkIdentity identity) => _ownerQueue.Remove(identity);

	public static void ServerUpdateOwnerQueue(this NetworkIdentity identity, uint id, OwnerQueueAction action)
	{
		var ownerQueue = _ownerQueue[identity];
		var oldOwner = ownerQueue.Count != 0 ? ownerQueue[0] : uint.MaxValue;

		switch (action)
		{
			case OwnerQueueAction.Add:
				ownerQueue.SafeAdd(id);
				break;

			case OwnerQueueAction.Remove:
				ownerQueue.Remove(id);
				break;

			case OwnerQueueAction.Force:
				ownerQueue.Remove(id);
				ownerQueue.Insert(0, id);
				break;
		}

		var newOwner = ownerQueue.Count != 0 ? ownerQueue[0] : uint.MaxValue;
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
		foreach (var identity in _ownerQueue.Keys)
		{
			identity.ServerUpdateOwnerQueue(id, OwnerQueueAction.Remove);
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

	public static void UpdateOwnerQueue(this NetworkIdentity identity, OwnerQueueAction action)
	{
		new OwnerQueueMessage(identity.netId, action).Send();
	}

	#endregion
}
