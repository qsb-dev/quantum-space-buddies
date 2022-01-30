using QSB.Messaging;

namespace QSB.ClientServerStateSync.Messages
{
	internal class ServerStateMessage : QSBMessage<ServerState>
	{
		public ServerStateMessage(ServerState state) => Value = state;

		public override void OnReceiveLocal() => OnReceiveRemote();
		public override void OnReceiveRemote()
			=> ServerStateManager.Instance.ChangeServerState(Value);
	}
}
