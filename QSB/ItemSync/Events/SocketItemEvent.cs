using QSB.Events;
using QSB.ItemSync.WorldObjects;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync.Events
{
	internal class SocketItemEvent : QSBEvent<SocketItemMessage>
	{
		public override EventType Type => EventType.SocketItem;

		public override void SetupListener()
			=> GlobalMessenger<int, int, bool>.AddListener(EventNames.QSBSocketItem, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int, int, bool>.RemoveListener(EventNames.QSBSocketItem, Handler);

		private void Handler(int socketId, int itemId, bool inserting)
			=> SendEvent(CreateMessage(socketId, itemId, inserting));

		private SocketItemMessage CreateMessage(int socketId, int itemId, bool inserting) => new SocketItemMessage
		{
			AboutId = QSBPlayerManager.LocalPlayerId,
			SocketId = socketId,
			ItemId = itemId,
			Inserting = inserting
		};

		public override void OnReceiveRemote(bool server, SocketItemMessage message)
		{
			var socketWorldObject = QSBWorldSync.GetWorldFromId<IQSBOWItemSocket>(message.SocketId);
			var itemWorldObject = QSBWorldSync.GetWorldFromId<IQSBOWItem>(message.ItemId);
			if (message.Inserting)
			{
				socketWorldObject.PlaceIntoSocket(itemWorldObject);
				return;
			}
			socketWorldObject.RemoveFromSocket();
		}
	}
}
