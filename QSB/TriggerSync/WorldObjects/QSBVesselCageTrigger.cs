using QSB.EyeOfTheUniverse.VesselSync;
using QSB.Player;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBVesselCageTrigger : QSBTrigger<VesselWarpController>
	{
		public override void Init()
		{
			base.Init();
			AttachedObject.OnExit -= TriggerOwner.OnExitCageTrigger;
		}

		protected override void OnEnter(PlayerInfo player) => VesselManager.Instance.Enter(player);

		protected override void OnExit(PlayerInfo player) => VesselManager.Instance.Exit(player);
	}
}
