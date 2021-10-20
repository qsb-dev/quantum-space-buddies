using QSB.Events;
using QSB.Messaging;

namespace QSB.Utility.Events
{
	public class DebugEvent : QSBEvent<EnumMessage<DebugEventEnum>>
	{
		public override EventType Type => EventType.DebugEvent;

		public override void SetupListener() => GlobalMessenger<DebugEventEnum>.AddListener(EventNames.QSBDebugEvent, Handler);
		public override void CloseListener() => GlobalMessenger<DebugEventEnum>.RemoveListener(EventNames.QSBDebugEvent, Handler);

		private void Handler(DebugEventEnum type) => SendEvent(CreateMessage(type));

		private EnumMessage<DebugEventEnum> CreateMessage(DebugEventEnum type) => new EnumMessage<DebugEventEnum>
		{
			AboutId = LocalPlayerId,
			EnumValue = type
		};

		public override void OnReceiveLocal(bool isHost, EnumMessage<DebugEventEnum> message)
		{
			OnReceiveRemote(isHost, message);
		}

		public override void OnReceiveRemote(bool isHost, EnumMessage<DebugEventEnum> message)
		{
			switch (message.EnumValue)
			{
				case DebugEventEnum.TriggerSupernova:
					TimeLoop.SetSecondsRemaining(0f);
					break;
			}
		}
	}
}
