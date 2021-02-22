using OWML.Common;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using System.Linq;

namespace QSB.Player.Events
{
	public class PlayerReadyEvent : QSBEvent<ToggleMessage>
	{
		public override EventType Type => EventType.PlayerReady;

		public override void SetupListener() => GlobalMessenger<bool>.AddListener(EventNames.QSBPlayerReady, Handler);
		public override void CloseListener() => GlobalMessenger<bool>.RemoveListener(EventNames.QSBPlayerReady, Handler);

		private void Handler(bool ready) => SendEvent(CreateMessage(ready));

		private ToggleMessage CreateMessage(bool ready) => new ToggleMessage
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
			DebugLog.DebugWrite($"Get ready event from {message.FromId}", MessageType.Success);
			PlayerManager.GetPlayer(message.AboutId).IsReady = message.ToggleValue;
			EventManager.FireEvent(EventNames.QSBServerSendPlayerStates);
		}

		private void HandleClient(ToggleMessage message)
		{
			DebugLog.DebugWrite($"Get ready event from {message.FromId}", MessageType.Success);
			if (!PlayerManager.PlayerExists(message.FromId))
			{
				DebugLog.ToConsole(
					"Error - Got ready event for non-existent player! Did we not send a PlayerStatesRequestEvent? Or was it not handled?",
					MessageType.Error);
				return;
			}
			foreach (var item in PlayerManager.GetSyncObjects<TransformSync.TransformSync>()
				.Where(x => x != null && x.IsReady && x.ReferenceSector != null && x.PlayerId == LocalPlayerId))
			{
				EventManager.FireEvent(EventNames.QSBSectorChange, item.NetId.Value, item.ReferenceSector);
			}
		}
	}
}