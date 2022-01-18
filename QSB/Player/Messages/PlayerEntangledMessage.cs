using Mirror;
using QSB.Messaging;
using QSB.QuantumSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Player.Messages
{
	// almost a world object message, but supports null (-1) as well
	internal class PlayerEntangledMessage : QSBMessage
	{
		private int ObjectId;

		public PlayerEntangledMessage(int objectId) => ObjectId = objectId;

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveLocal()
		{
			var player = QSBPlayerManager.LocalPlayer;
			if (ObjectId == -1)
			{
				player.EntangledObject = null;
				return;
			}

			var quantumObject = ObjectId.GetWorldObject<IQSBQuantumObject>();
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

			var quantumObject = ObjectId.GetWorldObject<IQSBQuantumObject>();
			player.EntangledObject = quantumObject;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(ObjectId);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			ObjectId = reader.Read<int>();
		}
	}
}
