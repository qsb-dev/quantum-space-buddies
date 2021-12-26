using QSB.Messaging;
using QSB.WorldSync;
using QuantumUNET;
using QuantumUNET.Transport;

namespace QSB.AuthoritySync
{
	/// <summary>
	/// always sent to host
	/// </summary>
	public class AuthQueueMessage : QSBEnumMessage<AuthQueueAction>
	{
		private QNetworkInstanceId NetId;

		public AuthQueueMessage(QNetworkInstanceId netId, AuthQueueAction action)
		{
			To = 0;
			NetId = netId;
			Value = action;
		}

		public AuthQueueMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(NetId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			NetId = reader.ReadNetworkId();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;
		public override void OnReceiveLocal() => OnReceiveRemote();
		public override void OnReceiveRemote() => QNetworkServer.objects[NetId].UpdateAuthQueue(From, Value);
	}

	public enum AuthQueueAction
	{
		/// <summary>
		/// add id to the queue
		/// </summary>
		Add,
		/// <summary>
		/// remove id from the queue
		/// </summary>
		Remove,
		/// <summary>
		/// add id to the queue and force it to the front
		/// </summary>
		Force
	}
}
