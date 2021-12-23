using QSB.Events;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Components;
using System;
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

		private static readonly List<Tuple<QNetworkIdentity, uint, AuthQueueAction>> _waitingQueue = new();

		public static void RegisterAuthQueue(this QNetworkIdentity identity)
		{
			_authQueue.Add(identity, new List<uint>());

			if (_waitingQueue.Any(x => x.Item1 == identity))
			{
				if (_waitingQueue.Count(x => x.Item1 == identity) > 1)
				{
					DebugLog.ToConsole($"Warning - There are multiple queued actions for {identity.NetId.Value}. Picking the first one.", OWML.Common.MessageType.Warning);
				}

				var tuple = _waitingQueue.First(x => x.Item1 == identity);
				identity.UpdateAuthQueue(tuple.Item2, tuple.Item3);
				_waitingQueue.Remove(tuple);
			}
		}
		
		public static void UnregisterAuthQueue(this QNetworkIdentity identity) => _authQueue.Remove(identity);

		public static void UpdateAuthQueue(this QNetworkIdentity identity, uint id, AuthQueueAction action)
		{
			if (!_authQueue.ContainsKey(identity))
			{
				DebugLog.ToConsole($"Error - Auth queue does not contain identity {identity.NetId.Value}. Adding to wait list.", OWML.Common.MessageType.Error);
				_waitingQueue.Add(new Tuple<QNetworkIdentity, uint, AuthQueueAction>(identity, id, action));
				return;
			}

			var authQueue = _authQueue[identity];
			var oldOwner = authQueue.Count != 0 ? authQueue[0] : uint.MaxValue;

			switch (action)
			{
				case AuthQueueAction.Add:
					authQueue.SafeAdd(id);
					break;

				case AuthQueueAction.Remove:
					authQueue.Remove(id);
					break;

				case AuthQueueAction.Force:
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
