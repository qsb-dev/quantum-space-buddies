using QSB.Messaging;
using QuantumUNET.Components;
using QuantumUNET.Transport;

namespace QSB.AuthoritySync
{
	public class AuthQueueMessage : EnumMessage<AuthQueueAction>
	{
		public QNetworkIdentity Identity;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Identity = reader.ReadNetworkIdentity();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Identity);
		}
	}

	public enum AuthQueueAction
	{
		/// <summary>
		/// add identity to the queue
		/// </summary>
		Add,
		/// <summary>
		/// remove identity from the queue
		/// </summary>
		Remove,
		/// <summary>
		/// add identity to the queue and force it to the front
		/// </summary>
		Force
	}
}
