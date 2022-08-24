using QSB.Messaging;

namespace QSB.Player.Messages;

internal class PlayerHeartbeatMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		if (!QSBCore.IsHost)
		{
			// send a response back to the host
			new PlayerHeartbeatMessage { To = 0 }.Send();
		}
		else
		{
			// note that we got a response
			var player = QSBPlayerManager.GetPlayer(From);
			player.HeartbeatReceived = true;
		}
	}
}
