using QSB.EyeOfTheUniverse.MaskSync;
using QSB.Player;

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

		protected override void OnEnter(PlayerInfo player) => MaskManager.Instance.Enter(player);

		protected override void OnExit(PlayerInfo player) => MaskManager.Instance.Exit(player);
	}
}
