using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;

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

		public SocketItemMessage() { }

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(SocketId);
			writer.Write(ItemId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			SocketId = reader.ReadInt32();
			ItemId = reader.ReadInt32();
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
					socketWorldObject = QSBWorldSync.GetWorldFromId<IQSBOWItemSocket>(SocketId);
					itemWorldObject = QSBWorldSync.GetWorldFromId<IQSBOWItem>(ItemId);

					socketWorldObject.PlaceIntoSocket(itemWorldObject);
					return;
				case SocketMessageType.StartUnsocket:
					socketWorldObject = QSBWorldSync.GetWorldFromId<IQSBOWItemSocket>(SocketId);

					if (!socketWorldObject.IsSocketOccupied())
					{
						DebugLog.ToConsole($"Warning - Trying to start unsocket on socket that is unoccupied! Socket:{socketWorldObject.Name}");
						return;
					}

					socketWorldObject.RemoveFromSocket();
					return;
				case SocketMessageType.CompleteUnsocket:
					itemWorldObject = QSBWorldSync.GetWorldFromId<IQSBOWItem>(ItemId);

					itemWorldObject.OnCompleteUnsocket();
					return;
			}
		}
	}
}
