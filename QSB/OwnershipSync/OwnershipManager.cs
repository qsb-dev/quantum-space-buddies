using Mirror;
using QSB.Messaging;
using QSB.Utility;
using System.Collections.Generic;

namespace QSB.OwnershipSync;

public static class OwnershipManager
{
	#region host only

	/// <summary>
	/// whoever is first gets authority
	/// </summary>
	private static readonly Dictionary<NetworkIdentity, List<uint>> _authQueue = new();

	public static void RegisterAuthQueue(this NetworkIdentity identity) => _authQueue.Add(identity, new List<uint>());
	public static void UnregisterAuthQueue(this NetworkIdentity identity) => _authQueue.Remove(identity);

	public static void ServerUpdateAuthQueue(this NetworkIdentity identity, uint id, OwnQueueAction action)
	{
		var authQueue = _authQueue[identity];
		var oldOwner = authQueue.Count != 0 ? authQueue[0] : uint.MaxValue;

		switch (action)
		{
			case OwnQueueAction.Add:
				authQueue.SafeAdd(id);
				break;

			case OwnQueueAction.Remove:
				authQueue.Remove(id);
				break;

			case OwnQueueAction.Force:
				authQueue.Remove(id);
				authQueue.Insert(0, id);
				break;
		}

		var newOwner = authQueue.Count != 0 ? authQueue[0] : uint.MaxValue;
		if (oldOwner != newOwner)
		{
			SetAuthority(identity, newOwner);
		}
	}

	/// <summary>
	/// transfer authority to a different client
	/// </summary>
	public static void OnDisconnect(NetworkConnectionToClient conn)
	{
		var id = conn.GetPlayerId();
		foreach (var identity in _authQueue.Keys)
		{
			identity.ServerUpdateAuthQueue(id, OwnQueueAction.Remove);
		}
	}

	public static void SetAuthority(this NetworkIdentity identity, uint id)
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

	public static void UpdateAuthQueue(this NetworkIdentity identity, OwnQueueAction action)
	{
		if (action == OwnQueueAction.Force && identity.isOwned)
		{
			return;
		}

		new OwnQueueMessage(identity.netId, action).Send();
	}

	#endregion
}
