using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.Player;

namespace QSB.RespawnSync.Messages;

internal class PlayerRespawnMessage : QSBMessage<uint>
{
	public PlayerRespawnMessage(uint playerId) => Value = playerId;

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		if (Value == QSBPlayerManager.LocalPlayerId)
		{
			RespawnManager.Instance.Respawn();
			ClientStateManager.Instance.OnRespawn();
		}

		RespawnManager.Instance.OnPlayerRespawn(QSBPlayerManager.GetPlayer(Value));
	}
}