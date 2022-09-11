using QSB.Animation.Player;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync.Messages;

internal class SocketItemMessage : QSBMessage<(SocketMessageType Type, int SocketId, int ItemId)>
{
	public SocketItemMessage(SocketMessageType type, OWItemSocket socket, OWItem item) : base((
		type,
		socket ? socket.GetWorldObject<QSBItemSocket>().ObjectId : -1,
		item ? item.GetWorldObject<IQSBItem>().ObjectId : -1
	)) { }

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		switch (Data.Type)
		{
			case SocketMessageType.Socket:
				{
					var qsbItemSocket = Data.SocketId.GetWorldObject<QSBItemSocket>();
					var qsbItem = Data.ItemId.GetWorldObject<IQSBItem>();

					qsbItemSocket.PlaceIntoSocket(qsbItem);
					qsbItem.ItemState.HasBeenInteractedWith = true;
					qsbItem.ItemState.State = ItemStateType.Socketed;
					qsbItem.ItemState.Socket = qsbItemSocket.AttachedObject;

					var player = QSBPlayerManager.GetPlayer(From);
					player.HeldItem = null;
					player.AnimationSync.VisibleAnimator.SetTrigger(AnimationSync.DROP_HELD_ITEM);
					return;
				}
			case SocketMessageType.StartUnsocket:
				{
					var qsbItemSocket = Data.SocketId.GetWorldObject<QSBItemSocket>();
					var qsbItem = Data.ItemId.GetWorldObject<IQSBItem>();

					if (!qsbItemSocket.IsSocketOccupied())
					{
						DebugLog.ToConsole($"Warning - Trying to start unsocket on socket that is unoccupied! Socket:{qsbItemSocket.Name}");
						return;
					}

					qsbItem.StoreLocation();

					var player = QSBPlayerManager.GetPlayer(From);
					player.HeldItem = qsbItem;

					qsbItemSocket.RemoveFromSocket();
					return;
				}
			case SocketMessageType.CompleteUnsocket:
				{
					var qsbItem = Data.ItemId.GetWorldObject<IQSBItem>();

					qsbItem.OnCompleteUnsocket();
					return;
				}
		}
	}
}
