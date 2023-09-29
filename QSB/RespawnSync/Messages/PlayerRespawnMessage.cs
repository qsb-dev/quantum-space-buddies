using QSB.ClientServerStateSync;
using QSB.Messaging;
using QSB.Player;

namespace QSB.RespawnSync.Messages;

public class PlayerRespawnMessage : QSBMessage<uint>
{
	public PlayerRespawnMessage(uint playerId) : base(playerId) { }

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		if (Data == QSBPlayerManager.LocalPlayerId)
		{
			RespawnManager.Instance.Respawn();
			ClientStateManager.Instance.OnRespawn();
		}

		RespawnManager.Instance.OnPlayerRespawn(QSBPlayerManager.GetPlayer(Data));
	}
}