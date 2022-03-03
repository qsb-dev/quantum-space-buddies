using QSB.Messaging;

namespace QSB.ClientServerStateSync.Messages
{
	/// <summary>
	/// sets the state both locally and remotely
	/// </summary>
	internal class ServerStateMessage : QSBMessage<ServerState>
	{
		public ServerStateMessage(ServerState state) => Data = state;

		public override void OnReceiveLocal() => OnReceiveRemote();
		public override void OnReceiveRemote()
			=> ServerStateManager.Instance.ChangeServerState(Data);
	}
}