using QSB.Player;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBEyeShuttleTrigger : QSBTrigger<EyeShuttleController>
	{
		public override void Init()
		{
			base.Init();
			AttachedObject.OnEntry -= TriggerOwner.OnEnterShuttle;
			AttachedObject.OnExit -= TriggerOwner.OnExitShuttle;
		}

		protected override void OnEnter(PlayerInfo player)
			=> player.IsInEyeShuttle = true;

		protected override void OnExit(PlayerInfo player)
			=> player.IsInEyeShuttle = false;
	}
}
