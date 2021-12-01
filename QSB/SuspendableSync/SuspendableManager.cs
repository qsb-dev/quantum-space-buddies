using System.Collections.Generic;
using System.Linq;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Components;

namespace QSB.SuspendableSync
{
	/// all of this is host only
	public static class SuspendableManager
	{
		private static readonly Dictionary<QNetworkIdentity, List<uint>> _unsuspendedFor = new();

		public static void Register(QNetworkIdentity identity) => _unsuspendedFor.Add(identity, new List<uint>());
		public static void Unregister(QNetworkIdentity identity) => _unsuspendedFor.Remove(identity);

		public static void UpdateSuspended(uint id, QNetworkIdentity identity, bool suspended)
		{
			var unsuspendedFor = _unsuspendedFor[identity];

			var oldSuspended = !unsuspendedFor.Contains(id);
			if (suspended == oldSuspended)
			{
				return;
			}

			if (!suspended)
			{
				unsuspendedFor.Add(id);
			}
			else
			{
				unsuspendedFor.Remove(id);
			}

			var newOwner = unsuspendedFor.Count != 0 ? unsuspendedFor[0] : uint.MaxValue;
			SetAuthority(identity, newOwner);
		}

		private static void SetAuthority(QNetworkIdentity identity, uint id)
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

			DebugLog.DebugWrite($"{identity.NetId}:{identity.gameObject.name} - set authority to {id}");
		}


		/// transfer authority to a different client
		public static void OnDisconnect(QNetworkConnection conn)
		{
			var id = conn.GetPlayerId();
			foreach (var (identity, unsuspendedFor) in _unsuspendedFor)
			{
				if (unsuspendedFor.Remove(id))
				{
					var newOwner = unsuspendedFor.Count != 0 ? unsuspendedFor[0] : uint.MaxValue;
					SetAuthority(identity, newOwner);
				}
			}
		}
	}
}
