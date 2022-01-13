using QSB.Animation.NPC.WorldObjects;
using QSB.Player;
using QSB.WorldSync;

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

		protected override void OnEnter(PlayerInfo player)
			=> TriggerOwner._solanumAnimController.GetWorldObject<QSBSolanumAnimController>().AddPlayerToHeadZone(player);

		protected override void OnExit(PlayerInfo player)
			=> TriggerOwner._solanumAnimController.GetWorldObject<QSBSolanumAnimController>().RemovePlayerFromHeadZone(player);
	}
}
