namespace QSB.TriggerSync.WorldObjects
{
	public class QSBCharacterTrigger : QSBTrigger<CharacterAnimController>
	{
		public override void Init()
		{
			base.Init();
			AttachedObject.OnEntry -= TriggerOwner.OnZoneEntry;
			AttachedObject.OnExit -= TriggerOwner.OnZoneExit;
		}

		public override void OnRemoval()
		{
			base.OnRemoval();
			AttachedObject.OnEntry += TriggerOwner.OnZoneEntry;
			AttachedObject.OnExit += TriggerOwner.OnZoneExit;
		}
	}
}
