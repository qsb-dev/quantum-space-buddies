using System.Collections.Generic;
using System.Linq;
using QSB.Events;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Components;

namespace QSB.SuspensionAuthoritySync
{
	public class SuspensionChangeEvent : QSBEvent<SuspensionChangeMessage>
	{
		private static Dictionary<QNetworkIdentity, List<uint>> _potentialOwners;

		public override void SetupListener()
		{
			if (QSBCore.IsHost)
			{
				_potentialOwners = new Dictionary<QNetworkIdentity, List<uint>>();
			}
			GlobalMessenger<QNetworkIdentity, bool>.AddListener(EventNames.QSBSuspensionChange, Handler);
		}

		public override void CloseListener()
		{
			if (QSBCore.IsHost)
			{
				_potentialOwners = null;
			}
			GlobalMessenger<QNetworkIdentity, bool>.RemoveListener(EventNames.QSBSuspensionChange, Handler);
		}

		private void Handler(QNetworkIdentity identity, bool suspended) => SendEvent(CreateMessage(identity, suspended));

		private SuspensionChangeMessage CreateMessage(QNetworkIdentity identity, bool suspended) => new()
		{
			OnlySendToHost = true,
			Identity = identity,
			Suspended = suspended
		};


		public override void OnReceiveLocal(bool isHost, SuspensionChangeMessage message) => OnReceive(message);
		public override void OnReceiveRemote(bool isHost, SuspensionChangeMessage message) => OnReceive(message);

		private static void OnReceive(SuspensionChangeMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			// init owner list for this identity
			if (!_potentialOwners.TryGetValue(message.Identity, out var potentialOwners))
			{
				potentialOwners = new List<uint>();
				_potentialOwners[message.Identity] = potentialOwners;
			}

			if (!message.Suspended)
			{
				potentialOwners.Add(message.FromId);
			}
			else
			{
				potentialOwners.Remove(message.FromId);
			}

			var newOwner = potentialOwners.Count != 0 ? potentialOwners[0] : uint.MaxValue;
			SetAuthority(message.Identity, newOwner);
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
	}
}
