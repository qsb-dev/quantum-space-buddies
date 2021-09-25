using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.ClientServerStateSync.Events
{
	internal class ClientStateEvent : QSBEvent<EnumMessage<ClientState>>
	{
		public override EventType Type => EventType.ClientState;

		public override void SetupListener()
			=> GlobalMessenger<ClientState>.AddListener(EventNames.QSBClientState, Handler);

		public override void CloseListener()
			=> GlobalMessenger<ClientState>.RemoveListener(EventNames.QSBClientState, Handler);

		private void Handler(ClientState state) => SendEvent(CreateMessage(state));

		private EnumMessage<ClientState> CreateMessage(ClientState state) => new EnumMessage<ClientState>
		{
			AboutId = LocalPlayerId,
			EnumValue = state
		};

		public override void OnReceiveLocal(bool server, EnumMessage<ClientState> message)
			=> ClientStateManager.Instance.ChangeClientState(message.EnumValue);

		public override void OnReceiveRemote(bool server, EnumMessage<ClientState> message)
		{
			if (message.AboutId == uint.MaxValue)
			{
				DebugLog.DebugWrite($"Error - ID is uint.MaxValue!", OWML.Common.MessageType.Error);
				return;
			}

			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.State = message.EnumValue;
		}
	}
}
