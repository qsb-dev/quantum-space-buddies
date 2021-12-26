using OWML.Common;
using QSB.CampfireSync.WorldObjects;
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
			GlobalMessenger<Campfire>.AddListener(OWEvents.EnterRoastingMode, campfire => Handler(campfire, true));
			GlobalMessenger.AddListener(OWEvents.ExitRoastingMode, () => Handler(null, false));
		}

		private static void Handler(Campfire campfire, bool roasting)
		{
			if (campfire == null)
			{
				new EnterExitRoastingMessage(-1, roasting).Send();
				return;
			}

			var qsbObj = campfire.GetWorldObject<QSBCampfire>();
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
				DebugLog.ToConsole($"Error - Null campfire supplied for start roasting event!", MessageType.Error);
				return;
			}

			var player = QSBPlayerManager.GetPlayer(From);
			player.RoastingStick.SetActive(Value);
			player.Campfire = Value
				? ObjectId.GetWorldObject<QSBCampfire>()
				: null;
		}
	}
}