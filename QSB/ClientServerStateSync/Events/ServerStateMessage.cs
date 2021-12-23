using QSB.Messaging;

namespace QSB.ClientServerStateSync.Events
{
	internal class ServerStateMessage : QSBEnumMessage<ServerState>
	{
		public ServerStateMessage(ServerState state) => Value = state;

		public ServerStateMessage() { }

		public override void OnReceiveLocal() => OnReceiveRemote();
		public override void OnReceiveRemote()
			=> ServerStateManager.Instance.ChangeServerState(Value);
	}
}
