using QSB.Messaging;
using QSB.Player;
using QuantumUNET.Components;
using QuantumUNET.Transport;

namespace QSB.AuthoritySync
{
	/// remember: send to host only
	public class AuthorityQueueEvent : QSBMessage
	{
		public QNetworkIdentity Identity;
		public bool Queue;

		public override void Serialize(QNetworkWriter writer)
		{
			writer.Write(Identity);
			writer.Write(Queue);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			Identity = reader.ReadNetworkIdentity();
			Queue = reader.ReadBoolean();
		}

		public override void OnReceiveRemote(uint from) => OnReceive(from);
		public override void OnReceiveLocal() => OnReceive(QSBPlayerManager.LocalPlayerId);
		private void OnReceive(uint from) => Identity.UpdateAuthQueue(from, Queue);
	}
}
