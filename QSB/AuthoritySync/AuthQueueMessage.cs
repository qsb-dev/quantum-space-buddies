using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.AuthoritySync
{
	/// <summary>
	/// always sent to host
	/// </summary>
	public class AuthQueueMessage : QSBMessage<AuthQueueAction>
	{
		private uint NetId;

		public AuthQueueMessage(uint netId, AuthQueueAction action)
		{
			To = 0;
			NetId = netId;
			Value = action;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(NetId);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			NetId = reader.ReadUInt();
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;
		public override void OnReceiveLocal() => OnReceiveRemote();
		public override void OnReceiveRemote() => NetworkServer.spawned[NetId].ServerUpdateAuthQueue(From, Value);
	}

	public enum AuthQueueAction
	{
		/// <summary>
		/// add player to the queue
		/// </summary>
		Add,
		/// <summary>
		/// remove player from the queue
		/// </summary>
		Remove,
		/// <summary>
		/// add player to the queue and force them to the front
		/// </summary>
		Force
	}
}