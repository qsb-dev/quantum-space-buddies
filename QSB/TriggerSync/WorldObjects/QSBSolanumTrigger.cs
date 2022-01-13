namespace QSB.TriggerSync.WorldObjects
{
	public class QSBSolanumTrigger : QSBTrigger<NomaiConversationManager>
	{
		public override void Init()
		{
			base.Init();
			AttachedObject.OnEntry -= TriggerOwner.OnEnterWatchVolume;
			AttachedObject.OnExit -= TriggerOwner.OnExitWatchVolume;
		}

		public override void OnRemoval()
		{
			base.OnRemoval();
			AttachedObject.OnEntry += TriggerOwner.OnEnterWatchVolume;
			AttachedObject.OnExit += TriggerOwner.OnExitWatchVolume;
		}
	}
}
