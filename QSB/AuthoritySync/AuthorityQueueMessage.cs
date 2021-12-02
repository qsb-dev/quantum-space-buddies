using QSB.Messaging;
using QuantumUNET.Components;
using QuantumUNET.Transport;

namespace QSB.AuthoritySync
{
	public class AuthorityQueueMessage : PlayerMessage
	{
		public QNetworkIdentity Identity;
		public bool Queue;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Identity = reader.ReadNetworkIdentity();
			Queue = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Identity);
			writer.Write(Queue);
		}
	}
}
