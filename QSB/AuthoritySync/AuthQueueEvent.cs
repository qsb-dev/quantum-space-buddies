using QSB.Events;
using QuantumUNET.Components;

namespace QSB.AuthoritySync
{
	public class AuthQueueEvent : QSBEvent<AuthQueueMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() =>
			GlobalMessenger<QNetworkIdentity, AuthQueueAction>.AddListener(EventNames.QSBAuthQueue, Handler);

		public override void CloseListener() =>
			GlobalMessenger<QNetworkIdentity, AuthQueueAction>.RemoveListener(EventNames.QSBAuthQueue, Handler);

		private void Handler(QNetworkIdentity identity, AuthQueueAction action) => SendEvent(CreateMessage(identity, action));

		private AuthQueueMessage CreateMessage(QNetworkIdentity identity, AuthQueueAction action) => new()
		{
			OnlySendToHost = true,
			Identity = identity,
			EnumValue = action
		};


		public override void OnReceiveLocal(bool isHost, AuthQueueMessage message) => OnReceive(message);
		public override void OnReceiveRemote(bool isHost, AuthQueueMessage message) => OnReceive(message);

		private static void OnReceive(AuthQueueMessage message)
		{
			message.Identity.UpdateAuthQueue(message.FromId, message.EnumValue);
		}
	}
}
