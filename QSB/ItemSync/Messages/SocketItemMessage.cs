using Mirror;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync.Messages
{
	internal class SocketItemMessage : QSBEnumMessage<SocketMessageType>
	{
		private int SocketId;
		private int ItemId;

		public SocketItemMessage(SocketMessageType type, int socketId = -1, int itemId = -1)
		{
			Value = type;
			SocketId = socketId;
			ItemId = itemId;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SocketId);
			writer.Write(ItemId);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			SocketId = reader.Read<int>();
			ItemId = reader.Read<int>();
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			IQSBOWItemSocket socketWorldObject;
			IQSBOWItem itemWorldObject;
			var player = QSBPlayerManager.GetPlayer(From);
			player.HeldItem = null;
			switch (Value)
			{
				case SocketMessageType.Socket:
					socketWorldObject = SocketId.GetWorldObject<IQSBOWItemSocket>();
					itemWorldObject = ItemId.GetWorldObject<IQSBOWItem>();

					socketWorldObject.PlaceIntoSocket(itemWorldObject);
					return;
				case SocketMessageType.StartUnsocket:
					socketWorldObject = SocketId.GetWorldObject<IQSBOWItemSocket>();

					if (!socketWorldObject.IsSocketOccupied())
					{
						DebugLog.ToConsole($"Warning - Trying to start unsocket on socket that is unoccupied! Socket:{socketWorldObject.Name}");
						return;
					}

					socketWorldObject.RemoveFromSocket();
					return;
				case SocketMessageType.CompleteUnsocket:
					itemWorldObject = ItemId.GetWorldObject<IQSBOWItem>();

					itemWorldObject.OnCompleteUnsocket();
					return;
			}
		}
	}
}
