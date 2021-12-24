using QSB.CampfireSync.WorldObjects;
using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.RoastingSync.Messages
{
	internal class EnterExitRoastingMessage : QSBBoolMessage
	{
		static EnterExitRoastingMessage()
		{
			GlobalMessenger<Campfire>.AddListener(EventNames.EnterRoastingMode, campfire => Handler(campfire, true));
			GlobalMessenger.AddListener(EventNames.ExitRoastingMode, () => Handler(null, false));
		}

		private static void Handler(Campfire campfire, bool roasting)
		{
			if (campfire == null)
			{
				new EnterExitRoastingMessage(-1, roasting).Send();
				return;
			}

			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCampfire>(campfire);
			new EnterExitRoastingMessage(qsbObj.ObjectId, roasting).Send();
		}

		private int ObjectId;

		private EnterExitRoastingMessage(int objectId, bool roasting)
		{
			ObjectId = objectId;
			Value = roasting;
		}

		public EnterExitRoastingMessage() { }

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

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			if (Value && ObjectId == -1)
			{
				DebugLog.ToConsole($"Error - Null campfire supplied for start roasting event!", OWML.Common.MessageType.Error);
				return;
			}

			var player = QSBPlayerManager.GetPlayer(From);
			player.RoastingStick.SetActive(Value);
			player.Campfire = Value
				? QSBWorldSync.GetWorldFromId<QSBCampfire>(ObjectId)
				: null;
		}
	}
}