using System.Linq;
using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QuantumUNET;
using QuantumUNET.Components;

namespace QSB.SuspendableSync
{
	public class SuspendChangeEvent : QSBEvent<SuspendChangeMessage>
	{
		public override void SetupListener() =>
			GlobalMessenger<QNetworkIdentity, bool>.AddListener(EventNames.QSBSuspendChange, Handler);

		public override void CloseListener() =>
			GlobalMessenger<QNetworkIdentity, bool>.RemoveListener(EventNames.QSBSuspendChange, Handler);

		private void Handler(QNetworkIdentity identity, bool suspended) => SendEvent(CreateMessage(identity, suspended));

		private SuspendChangeMessage CreateMessage(QNetworkIdentity identity, bool unsuspended) => new()
		{
			OnlySendToHost = true,
			Identity = identity,
			Suspended = unsuspended
		};


		public override void OnReceiveLocal(bool isHost, SuspendChangeMessage message) => OnReceive(message);
		public override void OnReceiveRemote(bool isHost, SuspendChangeMessage message) => OnReceive(message);

		private static void OnReceive(SuspendChangeMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var unsuspendedFor = SuspendableManager._unsuspendedFor[message.Identity];

			var suspended = !unsuspendedFor.Contains(message.FromId);
			if (message.Suspended == suspended)
			{
				return;
			}

			if (!message.Suspended)
			{
				unsuspendedFor.Add(message.FromId);
			}
			else
			{
				unsuspendedFor.Remove(message.FromId);
			}

			var newOwner = unsuspendedFor.Count != 0 ? unsuspendedFor[0] : uint.MaxValue;
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

			DebugLog.DebugWrite($"{QSBPlayerManager.LocalPlayerId}.{identity.NetId}:{identity.gameObject.name} - set authority to {id}");
		}
	}
}
