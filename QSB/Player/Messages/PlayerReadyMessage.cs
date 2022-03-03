using OWML.Common;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Player.Messages;

public class PlayerReadyMessage : QSBMessage<bool>
{
	public PlayerReadyMessage(bool ready) => Data = ready;

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
		DebugLog.DebugWrite($"[SERVER] Get ready event from {From} (ready = {Data})", MessageType.Success);
		QSBPlayerManager.GetPlayer(From).IsReady = Data;
		new PlayerInformationMessage().Send();
	}

	private void HandleClient()
	{
		DebugLog.DebugWrite($"[CLIENT] Get ready event from {From} (ready = {Data})", MessageType.Success);
		if (!QSBPlayerManager.PlayerExists(From))
		{
			DebugLog.ToConsole(
				"Error - Got ready event for non-existent player! Did we not send a PlayerStatesRequestEvent? Or was it not handled?",
				MessageType.Error);
			return;
		}

		QSBPlayerManager.GetPlayer(From).IsReady = Data;
	}
}