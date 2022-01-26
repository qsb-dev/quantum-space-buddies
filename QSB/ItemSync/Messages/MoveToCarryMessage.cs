using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;

namespace QSB.ItemSync.Messages
{
	internal class MoveToCarryMessage : QSBWorldObjectMessage<QSBItem>
	{
		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			var itemType = WorldObject.GetItemType();

			player.HeldItem = WorldObject;
			var itemSocket = itemType switch
			{
				ItemType.Scroll => player.ScrollSocket,
				ItemType.SharedStone => player.SharedStoneSocket,
				ItemType.WarpCore => ((WarpCoreItem)WorldObject.AttachedObject).IsVesselCoreType()
					? player.VesselCoreSocket
					: player.WarpCoreSocket,
				ItemType.Lantern => player.SimpleLanternSocket,
				ItemType.DreamLantern => player.DreamLanternSocket,
				ItemType.SlideReel => player.SlideReelSocket,
				ItemType.VisionTorch => player.VisionTorchSocket,
				_ => player.ItemSocket,
			};
			WorldObject.PickUpItem(itemSocket);
		}
	}
}
