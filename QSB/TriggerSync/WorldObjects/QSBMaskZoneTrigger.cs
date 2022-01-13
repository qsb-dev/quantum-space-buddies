namespace QSB.TriggerSync.WorldObjects
{
	public class QSBMaskZoneTrigger : QSBTrigger<MaskZoneController>
	{
		public override void Init()
		{
			base.Init();
			AttachedObject.OnEntry -= TriggerOwner.OnEnterMaskZone;
			AttachedObject.OnExit -= TriggerOwner.OnExitMaskZone;
		}

		public override void OnRemoval()
		{
			base.OnRemoval();
			AttachedObject.OnEntry += TriggerOwner.OnEnterMaskZone;
			AttachedObject.OnExit += TriggerOwner.OnExitMaskZone;
		}
	}
}
