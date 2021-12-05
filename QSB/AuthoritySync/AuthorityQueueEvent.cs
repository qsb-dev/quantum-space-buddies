using QSB.Events;
using QSB.WorldSync;
using QuantumUNET.Components;

namespace QSB.AuthoritySync
{
	public class AuthorityQueueEvent : QSBEvent<AuthorityQueueMessage>
	{
		public override bool RequireWorldObjectsReady() => true;

		public override void SetupListener() =>
			GlobalMessenger<QNetworkIdentity, bool>.AddListener(EventNames.QSBAuthorityQueue, Handler);

		public override void CloseListener() =>
			GlobalMessenger<QNetworkIdentity, bool>.RemoveListener(EventNames.QSBAuthorityQueue, Handler);

		private void Handler(QNetworkIdentity identity, bool queue) => SendEvent(CreateMessage(identity, queue));

		private AuthorityQueueMessage CreateMessage(QNetworkIdentity identity, bool queue) => new()
		{
			OnlySendToHost = true,
			Identity = identity,
			Queue = queue
		};


		public override void OnReceiveLocal(bool isHost, AuthorityQueueMessage message) => OnReceive(message);
		public override void OnReceiveRemote(bool isHost, AuthorityQueueMessage message) => OnReceive(message);

		private static void OnReceive(AuthorityQueueMessage message)
		{
			message.Identity.UpdateAuthQueue(message.FromId, message.Queue);
		}
	}
}
