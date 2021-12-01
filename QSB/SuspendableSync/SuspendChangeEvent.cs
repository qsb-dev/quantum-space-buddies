using QSB.Events;
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

			SuspendableManager.UpdateSuspended(message.FromId, message.Identity, message.Suspended);
		}
	}
}
