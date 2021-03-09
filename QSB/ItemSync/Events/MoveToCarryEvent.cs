using QSB.Events;
using QSB.ItemSync.WorldObjects;
using QSB.Player;
using QSB.WorldSync;
using QSB.WorldSync.Events;
using UnityEngine;

namespace QSB.ItemSync.Events
{
	internal class MoveToCarryEvent : QSBEvent<WorldObjectMessage>
	{
		public override QSB.Events.EventType Type => QSB.Events.EventType.MoveToCarry;

		public override void SetupListener()
			=> GlobalMessenger<int>.AddListener(EventNames.QSBMoveToCarry, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int>.RemoveListener(EventNames.QSBMoveToCarry, Handler);

		private void Handler(int itemId)
			=> SendEvent(CreateMessage(itemId));

		private WorldObjectMessage CreateMessage(int itemid) => new WorldObjectMessage
		{
			AboutId = QSBPlayerManager.LocalPlayerId,
			ObjectId = itemid
		};

		public override void OnReceiveRemote(bool server, WorldObjectMessage message)
		{
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			var itemObject = QSBWorldSync.GetWorldFromId<IQSBOWItem>(message.ObjectId);
			var itemType = itemObject.GetItemType();
			Transform itemSocket = null;
			switch (itemType)
			{
				case ItemType.Scroll:
					itemSocket = player.ScrollSocket;
					break;
				case ItemType.SharedStone:
					itemSocket = player.SharedStoneSocket;
					break;
				case ItemType.WarpCore:
					itemSocket = ((QSBWarpCoreItem)itemObject).IsVesselCoreType() 
						? player.VesselCoreSocket 
						: player.WarpCoreSocket;
					break;
				default:
					itemSocket = player.ItemSocket;
					break;

			}
			itemObject.PickUpItem(itemSocket);
		}
	}
}
