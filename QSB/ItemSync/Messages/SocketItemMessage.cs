using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync.Messages;

public class SocketItemMessage : QSBWorldObjectMessage<IQSBItem, (SocketMessageType Type, int SocketId)>
{
	public SocketItemMessage(SocketMessageType type, OWItemSocket socket) : base((
		type,
		socket ? socket.GetWorldObject<QSBItemSocket>().ObjectId : -1
	)) { }

	public override void OnReceiveRemote()
	{
		switch (Data.Type)
		{
			case SocketMessageType.Socket:
				{
					var qsbItemSocket = Data.SocketId.GetWorldObject<QSBItemSocket>();

					qsbItemSocket.PlaceIntoSocket(WorldObject);
					WorldObject.ItemState.HasBeenInteractedWith = true;
					WorldObject.ItemState.State = ItemStateType.Socketed;
					WorldObject.ItemState.Socket = qsbItemSocket.AttachedObject;

					var player = QSBPlayerManager.GetPlayer(From);
					player.HeldItem = null;
					player.AnimationSync.VisibleAnimator.SetTrigger("DropHeldItem");
					return;
				}
			case SocketMessageType.StartUnsocket:
				{
					var qsbItemSocket = Data.SocketId.GetWorldObject<QSBItemSocket>();

					if (!qsbItemSocket.IsSocketOccupied())
					{
						DebugLog.ToConsole($"Warning - Trying to start unsocket on socket that is unoccupied! Socket:{qsbItemSocket.Name}");
						return;
					}

					WorldObject.StoreLocation();

					var player = QSBPlayerManager.GetPlayer(From);
					player.HeldItem = WorldObject;

					qsbItemSocket.RemoveFromSocket();
					return;
				}
			case SocketMessageType.CompleteUnsocket:
				{
					WorldObject.OnCompleteUnsocket();
					return;
				}
		}
	}
}
