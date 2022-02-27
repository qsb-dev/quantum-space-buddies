using QSB.Player;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBEyeShuttleTrigger : QSBTrigger<EyeShuttleController>
	{
		protected override void OnEnter(PlayerInfo player)
			=> player.IsInEyeShuttle = true;

		protected override void OnExit(PlayerInfo player)
			=> player.IsInEyeShuttle = false;
	}
}