using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.ItemSync.Messages
{
	internal class MoveToCarryMessage : QSBWorldObjectMessage<IQSBItem>
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
				ItemType.WarpCore => ((QSBWarpCoreItem)WorldObject).IsVesselCoreType()
					? player.VesselCoreSocket
					: player.WarpCoreSocket,
				ItemType.Lantern => player.SimpleLanternSocket,
				ItemType.DreamLantern => player.DreamLanternSocket,
				ItemType.SlideReel => player.SlideReelSocket,
				ItemType.VisionTorch => player.VisionTorchSocket,
				_ => player.ItemSocket,
			};
			WorldObject.PickUpItem(itemSocket);

			switch (itemType)
			{
				case ItemType.Scroll:
					DebugLog.DebugWrite($"HOLD SCROLL");
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldScroll");
					break;
				case ItemType.WarpCore:
					if ((WorldObject as QSBWarpCoreItem).IsVesselCoreType())
					{
						DebugLog.DebugWrite($"HOLD VESSEL CORE");
						player.AnimationSync.VisibleAnimator.SetTrigger("HoldAdvWarpCore");
					}
					else
					{
						DebugLog.DebugWrite($"HOLD WARP CORE");
						player.AnimationSync.VisibleAnimator.SetTrigger("HoldWarpCore");
					}

					break;
				case ItemType.SharedStone:
					DebugLog.DebugWrite($"HOLD SHARED STONE");
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldSharedStone");
					break;
				case ItemType.ConversationStone:
					break;
				case ItemType.Lantern:
					DebugLog.DebugWrite($"HOLD LANTERN");
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldLantern");
					break;
				case ItemType.SlideReel:
					break;
				case ItemType.DreamLantern:
					break;
				case ItemType.VisionTorch:
					break;
			}
		}
	}
}
