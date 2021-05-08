using QSB.CampfireSync.WorldObjects;
using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using QSB.WorldSync.Events;

namespace QSB.RoastingSync.Events
{
	internal class EnterExitRoastingEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override EventType Type => EventType.Roasting;

		public override void SetupListener()
		{
			GlobalMessenger<Campfire>.AddListener(EventNames.EnterRoastingMode, (Campfire fire) => Handler(fire, true));
			GlobalMessenger.AddListener(EventNames.ExitRoastingMode, () => Handler(null, false));
		}

		public override void CloseListener()
		{
			GlobalMessenger<Campfire>.RemoveListener(EventNames.EnterRoastingMode, (Campfire fire) => Handler(fire, true));
			GlobalMessenger.RemoveListener(EventNames.ExitRoastingMode, () => Handler(null, false));
		}

		private void Handler(Campfire campfire, bool roasting)
		{
			if (campfire == null)
			{
				SendEvent(CreateMessage(-1, roasting));
				return;
			}
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCampfire, Campfire>(campfire);
			SendEvent(CreateMessage(qsbObj.ObjectId, roasting));
		}

		private BoolWorldObjectMessage CreateMessage(int objectId, bool roasting) => new BoolWorldObjectMessage
		{
			AboutId = LocalPlayerId,
			State = roasting,
			ObjectId = objectId
		};

		public override void OnReceiveRemote(bool server, BoolWorldObjectMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}
			if (message.State && message.ObjectId == -1)
			{
				DebugLog.ToConsole($"Error - Null campfire supplied for start roasting event!", OWML.Common.MessageType.Error);
				return;
			}
			var player = QSBPlayerManager.GetPlayer(message.AboutId);
			player.RoastingStick.SetActive(message.State);
			if (message.State)
			{
				player.Campfire = QSBWorldSync.GetWorldFromId<QSBCampfire>(message.ObjectId);
			}
			else
			{
				player.Campfire = null;
			}
		}
	}
}
