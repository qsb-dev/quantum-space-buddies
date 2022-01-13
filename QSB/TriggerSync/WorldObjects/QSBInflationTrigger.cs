using QSB.EyeOfTheUniverse.CosmicInflation;
using QSB.Player;

namespace QSB.TriggerSync.WorldObjects
{
	public class QSBInflationTrigger : QSBTrigger<CosmicInflationController>
	{
		public override void Init()
		{
			base.Init();
			AttachedObject.OnEntry -= TriggerOwner.OnEnterFogSphere;
		}

		protected override void OnEnter(PlayerInfo player) => InflationManager.Instance.Enter(player);

		protected override void OnExit(PlayerInfo player) { }
	}
}
