using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.RoastingSync.Events
{
	class MarshmallowEventEvent : QSBEvent<EnumMessage<MarshmallowEventType>>
	{
		public override EventType Type => EventType.MarshmallowEvent;

		public override void SetupListener() => GlobalMessenger<MarshmallowEventType>.AddListener(EventNames.QSBMarshmallowEvent, Handler);
		public override void CloseListener() => GlobalMessenger<MarshmallowEventType>.RemoveListener(EventNames.QSBMarshmallowEvent, Handler);

		private void Handler(MarshmallowEventType type) => SendEvent(CreateMessage(type));

		private EnumMessage<MarshmallowEventType> CreateMessage(MarshmallowEventType type) => new EnumMessage<MarshmallowEventType>
		{
			AboutId = LocalPlayerId,
			EnumValue = type
		};

		public override void OnReceiveRemote(bool server, EnumMessage<MarshmallowEventType> message)
		{
			DebugLog.DebugWrite($"Get marshmallow event {message.EnumValue} from {message.AboutId}");
		}
	}
}
