using QSB.CampfireSync.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.CampfireSync.Messages;

public class BurnSlideReelMessage : QSBWorldObjectMessage<QSBSlideReelItem, int>
{
	public BurnSlideReelMessage(QSBCampfire campfire) : base(campfire.ObjectId) { }

	public override void OnReceiveRemote()
	{
		var campfire = Data.GetWorldObject<QSBCampfire>().AttachedObject;
		var fromPlayer = QSBPlayerManager.GetPlayer(From);
		WorldObject.DropItem(
			campfire._burnedSlideReelSocket.position,
			campfire._burnedSlideReelSocket.up,
			campfire._burnedSlideReelSocket,
			campfire._sector, null);
		fromPlayer.HeldItem = null;
		fromPlayer.AnimationSync.VisibleAnimator.SetTrigger("DropHeldItem");
		WorldObject.AttachedObject.Burn();
		campfire.SetDropSlideReelMode(false);
		campfire._hasBurnedSlideReel = true;
		campfire._oneShotAudio.PlayOneShot(AudioType.TH_Campfire_Ignite, 1f);
	}
}
