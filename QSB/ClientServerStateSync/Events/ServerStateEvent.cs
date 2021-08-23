using QSB.Events;
using QSB.Messaging;

namespace QSB.ClientServerStateSync.Events
{
	internal class ServerStateEvent : QSBEvent<EnumMessage<ServerState>>
	{
		public override EventType Type => EventType.ServerState;

		public override void SetupListener()
			=> GlobalMessenger<ServerState>.AddListener(EventNames.QSBServerState, Handler);

		public override void CloseListener()
			=> GlobalMessenger<ServerState>.RemoveListener(EventNames.QSBServerState, Handler);

		private void Handler(ServerState state) => SendEvent(CreateMessage(state));

		private EnumMessage<ServerState> CreateMessage(ServerState state) => new EnumMessage<ServerState>
		{
			AboutId = LocalPlayerId,
			EnumValue = state
		};

		public override void OnReceiveLocal(bool server, EnumMessage<ServerState> message)
			=> OnReceiveRemote(server, message);

		public override void OnReceiveRemote(bool server, EnumMessage<ServerState> message)
			=> ServerStateManager.Instance.ChangeServerState(message.EnumValue);
	}
}
