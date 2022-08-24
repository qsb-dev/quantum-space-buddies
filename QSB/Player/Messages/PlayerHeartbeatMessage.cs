using OWML.Common;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Player.Messages;

internal class PlayerHeartbeatMessage : QSBMessage
{
	public const int TOTAL_HEARTBEAT_TRIES = 2;

	public override void OnReceiveLocal()
	{
		if (QSBCore.IsHost)
		{
			var player = QSBPlayerManager.LocalPlayer;

			if (!player.WaitingForHeartbeat)
			{
				DebugLog.DebugWrite($"Wait a minute... a heartbeat was already handled from the local player!", MessageType.Warning);
			}

			player.HeartbeatsRemaining = TOTAL_HEARTBEAT_TRIES;
			player.WaitingForHeartbeat = false;
		}
	}

	public override void OnReceiveRemote()
	{
		if (QSBCore.IsHost)
		{
			var player = QSBPlayerManager.GetPlayer(From);

			if (!player.WaitingForHeartbeat)
			{
				DebugLog.DebugWrite($"Wait a minute... a heartbeat was already handled from {From}!", MessageType.Warning);
			}

			player.HeartbeatsRemaining = TOTAL_HEARTBEAT_TRIES;
			player.WaitingForHeartbeat = false;
			return;
		}

		new PlayerHeartbeatMessage() { To = 0 }.Send();
	}
}
