using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.ItemSync.Messages;

public class MoveToCarryMessage : QSBWorldObjectMessage<IQSBItem, uint>
{
	public MoveToCarryMessage(uint playerHolding) : base(playerHolding) { }

	public override void OnReceiveRemote()
	{
		WorldObject.StoreLocation();

		var player = QSBPlayerManager.GetPlayer(Data);
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
			_ => player.ItemSocket
		};

		WorldObject.PickUpItem(itemSocket);
		WorldObject.ItemState.HasBeenInteractedWith = true;
		WorldObject.ItemState.State = ItemStateType.Held;
		WorldObject.ItemState.HoldingPlayer = player;

		switch (itemType)
		{
			case ItemType.Scroll:
				player.AnimationSync.VisibleAnimator.SetTrigger("HoldScroll");
				break;
			case ItemType.WarpCore:
				if (((QSBWarpCoreItem)WorldObject).IsVesselCoreType())
				{
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldAdvWarpCore");
				}
				else
				{
					player.AnimationSync.VisibleAnimator.SetTrigger("HoldWarpCore");
				}

				break;
			case ItemType.SharedStone:
				player.AnimationSync.VisibleAnimator.SetTrigger("HoldSharedStone");
				break;
			case ItemType.ConversationStone:
				player.AnimationSync.VisibleAnimator.SetTrigger("HoldItem");
				break;
			case ItemType.Lantern:
				player.AnimationSync.VisibleAnimator.SetTrigger("HoldLantern");
				break;
			case ItemType.SlideReel:
			case ItemType.DreamLantern:
			case ItemType.VisionTorch:
				DebugLog.ToConsole($"Warning - {itemType} has no implemented holding pose.", OWML.Common.MessageType.Warning);
				break;
		}
	}
}
