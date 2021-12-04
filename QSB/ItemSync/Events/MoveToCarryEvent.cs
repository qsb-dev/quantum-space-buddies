﻿using QSB.Events;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Player;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.ItemSync.Events
{
	internal class MoveToCarryEvent : QSBEvent<WorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady() => true;

		public override void SetupListener()
			=> GlobalMessenger<int>.AddListener(EventNames.QSBMoveToCarry, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int>.RemoveListener(EventNames.QSBMoveToCarry, Handler);

		private void Handler(int itemId)
			=> SendEvent(CreateMessage(itemId));

		private WorldObjectMessage CreateMessage(int itemid) => new()
		{
			AboutId = QSBPlayerManager.LocalPlayerId,
			ObjectId = itemid
		};

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			var itemObject = QSBWorldSync.GetWorldFromId<IQSBOWItem>(message.ObjectId);
			var itemType = itemObject.GetItemType();

			player.HeldItem = itemObject;
			var itemSocket = itemType switch
			{
				ItemType.Scroll => player.ScrollSocket,
				ItemType.SharedStone => player.SharedStoneSocket,
				ItemType.WarpCore => ((QSBWarpCoreItem)itemObject).IsVesselCoreType()
					? player.VesselCoreSocket
					: player.WarpCoreSocket,
				ItemType.Lantern => player.SimpleLanternSocket,
				ItemType.DreamLantern => player.DreamLanternSocket,
				ItemType.SlideReel => player.SlideReelSocket,
				ItemType.VisionTorch => player.VisionTorchSocket,
				_ => player.ItemSocket,
			};
			itemObject.PickUpItem(itemSocket, message.AboutId);
		}
	}
}
