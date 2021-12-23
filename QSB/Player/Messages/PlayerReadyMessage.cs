using OWML.Common;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Player.Messages
{
	public class PlayerReadyMessage : QSBBoolMessage
	{
		public PlayerReadyMessage(bool ready) => Value = ready;

		public PlayerReadyMessage() { }

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
			new PlayerInformationMessage().Send();
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
