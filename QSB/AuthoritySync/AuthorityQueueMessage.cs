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
			base.Serialize(writer);
			writer.Write(Identity);
			writer.Write(Queue);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Identity = reader.ReadNetworkIdentity();
			Queue = reader.ReadBoolean();
		}

		public override void OnReceiveRemote() => OnReceive();
		public override void OnReceiveLocal() => OnReceive();
		private void OnReceive() => Identity.UpdateAuthQueue(From, Queue);
	}
}
