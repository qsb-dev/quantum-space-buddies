using System.Collections.Generic;
using QuantumUNET.Components;

namespace QSB.SuspendableSync
{
	/// all of this is host only
	public static class SuspendableManager
	{
		internal static readonly Dictionary<QNetworkIdentity, List<uint>> _unsuspendedFor = new();

		public static void Register(QNetworkIdentity identity) => _unsuspendedFor.Add(identity, new List<uint>());
		public static void Unregister(QNetworkIdentity identity) => _unsuspendedFor.Remove(identity);
	}
}
