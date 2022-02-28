using Mirror;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.AuthoritySync
{
	/// <summary>
	/// always sent to host
	/// </summary>
	public class AuthQueueMessage : QSBMessage<AuthQueueAction, uint>
	{
		public AuthQueueMessage(uint netId, AuthQueueAction action)
		{
			To = 0;
			Value1 = action;
			Value2 = netId;
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;
		public override void OnReceiveLocal() => OnReceiveRemote();
		public override void OnReceiveRemote() => NetworkServer.spawned[Value2].ServerUpdateAuthQueue(From, Value1);
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