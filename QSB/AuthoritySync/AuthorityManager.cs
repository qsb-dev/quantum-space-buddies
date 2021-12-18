using QSB.Events;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Components;
using System.Collections.Generic;
using System.Linq;

namespace QSB.AuthoritySync
{
	public static class AuthorityManager
	{
		#region host only

		/// <summary>
		/// whoever is first gets authority
		/// </summary>
		private static readonly Dictionary<QNetworkIdentity, List<uint>> _authQueue = new();

		public static void RegisterAuthQueue(this QNetworkIdentity identity) => _authQueue.Add(identity, new List<uint>());
		public static void UnregisterAuthQueue(this QNetworkIdentity identity) => _authQueue.Remove(identity);

		public static void UpdateAuthQueue(this QNetworkIdentity identity, uint id, AuthQueueAction action)
		{
			var authQueue = _authQueue[identity];

			switch (action)
			{
				case AuthQueueAction.Add:
					if (!authQueue.SafeAdd(id))
					{
						return;
					}
					break;

				case AuthQueueAction.Remove:
					if (!authQueue.Remove(id))
					{
						return;
					}
					break;

				case AuthQueueAction.Force:
					authQueue.Remove(id);
					authQueue.Insert(0, id);
					break;
			}

			var newOwner = authQueue.Count != 0 ? authQueue[0] : uint.MaxValue;
			SetAuthority(identity, newOwner);
		}

		/// <summary>
		/// transfer authority to a different client
		/// </summary>
		public static void OnDisconnect(uint id)
		{
			foreach (var identity in _authQueue.Keys)
			{
				identity.UpdateAuthQueue(id, AuthQueueAction.Remove);
			}
		}

		public static void SetAuthority(this QNetworkIdentity identity, uint id)
		{
			var oldConn = identity.ClientAuthorityOwner;
			var newConn = id != uint.MaxValue
				? QNetworkServer.connections.First(x => x.GetPlayerId() == id)
				: null;

			if (oldConn == newConn)
			{
				return;
			}

			if (oldConn != null)
			{
				identity.RemoveClientAuthority(oldConn);
			}

			if (newConn != null)
			{
				identity.AssignClientAuthority(newConn);
			}

			// DebugLog.DebugWrite($"{identity.NetId}:{identity.gameObject.name} - "
			// + $"set authority to {id}");
		}

		#endregion

		#region any client

		public static void FireAuthQueue(this QNetworkIdentity identity, AuthQueueAction action) =>
			QSBEventManager.FireEvent(EventNames.QSBAuthQueue, identity, action);

		#endregion
	}
}
