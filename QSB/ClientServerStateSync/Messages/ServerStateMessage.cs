using QSB.Messaging;

namespace QSB.ClientServerStateSync.Messages;

/// <summary>
/// sets the state both locally and remotely
/// </summary>
public class ServerStateMessage : QSBMessage<ServerState>
{
	public ServerStateMessage(ServerState state) : base(state) { }

	public override void OnReceiveLocal() => OnReceiveRemote();
	public override void OnReceiveRemote()
		=> ServerStateManager.Instance.ChangeServerState(Data);
}