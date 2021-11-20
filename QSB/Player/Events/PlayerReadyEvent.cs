using OWML.Common;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Player.Events
{
	public class PlayerReadyEvent : QSBEvent<ToggleMessage>
	{
		public override EventType Type => EventType.PlayerReady;

		public override void SetupListener() => GlobalMessenger<bool>.AddListener(EventNames.QSBPlayerReady, Handler);
		public override void CloseListener() => GlobalMessenger<bool>.RemoveListener(EventNames.QSBPlayerReady, Handler);

		private void Handler(bool ready) => SendEvent(CreateMessage(ready));

		private ToggleMessage CreateMessage(bool ready) => new()
		{
			AboutId = LocalPlayerId,
			ToggleValue = ready
		};

		public override void OnReceiveRemote(bool server, ToggleMessage message)
		{
			if (server)
			{
				HandleServer(message);
			}
			else
			{
				HandleClient(message);
			}
		}

		private static void HandleServer(ToggleMessage message)
		{
			DebugLog.DebugWrite($"[SERVER] Get ready event from {message.FromId}", MessageType.Success);
			QSBPlayerManager.GetPlayer(message.AboutId).IsReady = message.ToggleValue;
			QSBEventManager.FireEvent(EventNames.QSBPlayerInformation);
		}

		private void HandleClient(ToggleMessage message)
		{
			DebugLog.DebugWrite($"[CLIENT] Get ready event from {message.FromId}", MessageType.Success);
			if (!QSBPlayerManager.PlayerExists(message.FromId))
			{
				DebugLog.ToConsole(
					"Error - Got ready event for non-existent player! Did we not send a PlayerStatesRequestEvent? Or was it not handled?",
					MessageType.Error);
				return;
			}
		}
	}
}