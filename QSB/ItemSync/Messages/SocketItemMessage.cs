using Mirror;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync.Messages
{
	internal class SocketItemMessage : QSBMessage<SocketMessageType, int, int>
	{
		public SocketItemMessage(SocketMessageType type, int socketId = -1, int itemId = -1)
		{
			Value1 = type;
			Value2 = socketId;
			Value3 = itemId;
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

			switch (Value1)
			{
				case SocketMessageType.Socket:
					socketWorldObject = Value2.GetWorldObject<QSBItemSocket>();
					itemWorldObject = Value3.GetWorldObject<IQSBItem>();

					socketWorldObject.PlaceIntoSocket(itemWorldObject);
					return;
				case SocketMessageType.StartUnsocket:
					socketWorldObject = Value2.GetWorldObject<QSBItemSocket>();

					if (!socketWorldObject.IsSocketOccupied())
					{
						DebugLog.ToConsole($"Warning - Trying to start unsocket on socket that is unoccupied! Socket:{socketWorldObject.Name}");
						return;
					}

					socketWorldObject.RemoveFromSocket();
					return;
				case SocketMessageType.CompleteUnsocket:
					itemWorldObject = Value3.GetWorldObject<IQSBItem>();

					itemWorldObject.OnCompleteUnsocket();
					return;
			}
		}
	}
}