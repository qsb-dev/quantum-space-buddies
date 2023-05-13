using QSB.ClientServerStateSync;
using QSB.ClientServerStateSync.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;

namespace QSB.DeathSync.Messages;

// when all players die
internal class EndLoopMessage : QSBMessage
{
	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		DebugLog.DebugWrite($" ~~~~ END LOOP - all players are dead ~~~~ ");
		if (ServerStateManager.Instance.GetServerState() == ServerState.WaitingForAllPlayersToDie)
		{
			return;
		}

		QSBPatchManager.DoUnpatchType(QSBPatchTypes.SpectateTime);

		Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
		if (QSBCore.IsHost)
		{
			new ServerStateMessage(ServerState.WaitingForAllPlayersToDie).Send();
		}
	}
}