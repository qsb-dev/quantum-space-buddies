namespace QSB.TriggerSync.WorldObjects
{
	public class QSBVesselCageTrigger : QSBTrigger<VesselWarpController>
	{
		public override void Init()
		{
			base.Init();
			AttachedObject.OnExit -= TriggerOwner.OnExitCageTrigger;
		}

		public override void OnRemoval()
		{
			base.OnRemoval();
			AttachedObject.OnExit += TriggerOwner.OnExitCageTrigger;
		}
	}
}
