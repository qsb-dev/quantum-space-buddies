using QSB.Animation.NPC.WorldObjects;
using QSB.Player;
using QSB.WorldSync;

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

		protected override void OnEnter(PlayerInfo player)
			=> TriggerOwner.GetWorldObject<QSBCharacterAnimController>().AddPlayerToHeadZone(player);

		protected override void OnExit(PlayerInfo player)
			=> TriggerOwner.GetWorldObject<QSBCharacterAnimController>().RemovePlayerFromHeadZone(player);
	}
}
