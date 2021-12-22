using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Transport;

namespace QSB.AuthoritySync
{
	public class AuthQueueMessage : QSBEnumMessage<AuthQueueAction>
	{
		public QNetworkInstanceId NetId;

		public AuthQueueMessage() => To = 0;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			NetId = reader.ReadNetworkId();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(NetId);
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;
		public override void OnReceiveLocal() => QNetworkServer.objects[NetId].UpdateAuthQueue(From, Value);
		public override void OnReceiveRemote() => QNetworkServer.objects[NetId].UpdateAuthQueue(From, Value);
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
