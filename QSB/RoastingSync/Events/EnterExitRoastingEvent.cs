using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.RoastingSync.Events
{
	class EnterExitRoastingEvent : QSBEvent<BoolMessage>
	{
		public override EventType Type => EventType.Roasting;

		public override void SetupListener()
		{
			GlobalMessenger<Campfire>.AddListener(EventNames.EnterRoastingMode, (Campfire fire) => Handler(true));
			GlobalMessenger.AddListener(EventNames.ExitRoastingMode, () => Handler(false));
		}

		public override void CloseListener() 
		{
			GlobalMessenger<Campfire>.RemoveListener(EventNames.EnterRoastingMode, (Campfire fire) => Handler(true));
			GlobalMessenger.RemoveListener(EventNames.ExitRoastingMode, () => Handler(false));
		}

		private void Handler(bool roasting) => SendEvent(CreateMessage(roasting));

		private BoolMessage CreateMessage(bool roasting) => new BoolMessage
		{
			AboutId = LocalPlayerId,
			Value = roasting
		};

		public override void OnReceiveRemote(bool server, BoolMessage message)
		{
			DebugLog.DebugWrite($"Get roasting value {message.Value} for {message.AboutId}");
		}
	}
}
