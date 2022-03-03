using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync.Messages
{
	internal class SocketItemMessage : QSBMessage<(SocketMessageType Type, int SocketId, int ItemId)>
	{
		public SocketItemMessage(SocketMessageType type, int socketId = -1, int itemId = -1)
		{
			Data.Type = type;
			Data.SocketId = socketId;
			Data.ItemId = itemId;
		}

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			QSBItemSocket socketWorldObject;
			IQSBItem itemWorldObject;
			var player = QSBPlayerManager.GetPlayer(From);
			player.HeldItem = null;

			DebugLog.DebugWrite("DROP HELD ITEM");
			player.AnimationSync.VisibleAnimator.SetTrigger("DropHeldItem");

			switch (Data.Type)
			{
				case SocketMessageType.Socket:
					socketWorldObject = Data.SocketId.GetWorldObject<QSBItemSocket>();
					itemWorldObject = Data.ItemId.GetWorldObject<IQSBItem>();

					socketWorldObject.PlaceIntoSocket(itemWorldObject);
					return;
				case SocketMessageType.StartUnsocket:
					socketWorldObject = Data.SocketId.GetWorldObject<QSBItemSocket>();

					if (!socketWorldObject.IsSocketOccupied())
					{
						DebugLog.ToConsole($"Warning - Trying to start unsocket on socket that is unoccupied! Socket:{socketWorldObject.Name}");
						return;
					}

					socketWorldObject.RemoveFromSocket();
					return;
				case SocketMessageType.CompleteUnsocket:
					itemWorldObject = Data.ItemId.GetWorldObject<IQSBItem>();

					itemWorldObject.OnCompleteUnsocket();
					return;
			}
		}
	}
}
