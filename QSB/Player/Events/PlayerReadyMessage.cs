using OWML.Common;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Player.Events
{
	public class PlayerReadyMessage : QSBBoolMessage
	{
		public override void OnReceiveRemote()
		{
			if (QSBCore.IsHost)
			{
				HandleServer();
			}
			else
			{
				HandleClient();
			}
		}

		private void HandleServer()
		{
			DebugLog.DebugWrite($"[SERVER] Get ready event from {From} (ready = {Value})", MessageType.Success);
			QSBPlayerManager.GetPlayer(From).IsReady = Value;
			QSBEventManager.FireEvent(EventNames.QSBPlayerInformation);
		}

		private void HandleClient()
		{
			DebugLog.DebugWrite($"[CLIENT] Get ready event from {From} (ready = {Value})", MessageType.Success);
			if (!QSBPlayerManager.PlayerExists(From))
			{
				DebugLog.ToConsole(
					"Error - Got ready event for non-existent player! Did we not send a PlayerStatesRequestEvent? Or was it not handled?",
					MessageType.Error);
				return;
			}

			QSBPlayerManager.GetPlayer(From).IsReady = Value;
		}
	}
}