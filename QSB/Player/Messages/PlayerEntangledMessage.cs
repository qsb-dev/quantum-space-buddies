using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Player.Messages
{
	// almost a world object message, but supports null (-1) as well
	internal class PlayerEntangledMessage : QSBMessage
	{
		private int ObjectId;

		public PlayerEntangledMessage(int objectId) => ObjectId = objectId;

		public PlayerEntangledMessage() { }

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveLocal()
		{
			var player = QSBPlayerManager.LocalPlayer;
			if (ObjectId == -1)
			{
				player.EntangledObject = null;
				return;
			}

			var quantumObject = QSBWorldSync.GetWorldFromId<IQSBQuantumObject>(ObjectId);
			player.EntangledObject = quantumObject;
		}

		public override void OnReceiveRemote()
		{
			var player = QSBPlayerManager.GetPlayer(From);
			if (ObjectId == -1)
			{
				player.EntangledObject = null;
				return;
			}

			var quantumObject = QSBWorldSync.GetWorldFromId<IQSBQuantumObject>(ObjectId);
			player.EntangledObject = quantumObject;
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ObjectId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			ObjectId = reader.ReadInt32();
		}
	}
}
