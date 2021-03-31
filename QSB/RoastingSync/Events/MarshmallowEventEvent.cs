using QSB.Events;
using QSB.Messaging;
using QSB.Player;
using QSB.Utility;

namespace QSB.RoastingSync.Events
{
	internal class MarshmallowEventEvent : QSBEvent<EnumMessage<MarshmallowEventType>>
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
			var marshmallow = QSBPlayerManager.GetPlayer(message.AboutId).Marshmallow;
			if (marshmallow == null)
			{
				DebugLog.ToConsole($"Warning - Marshmallow is null for player {message.AboutId}.", OWML.Common.MessageType.Warning);
				return;
			}
			switch (message.EnumValue)
			{
				case MarshmallowEventType.Burn:
					marshmallow.Burn();
					break;
				case MarshmallowEventType.Extinguish:
					marshmallow.Extinguish();
					break;
				case MarshmallowEventType.Remove:
					marshmallow.RemoveMallow();
					break;
				case MarshmallowEventType.Replace:
					marshmallow.SpawnMallow();
					break;
				case MarshmallowEventType.Shrivel:
					marshmallow.Shrivel();
					break;
				case MarshmallowEventType.Toss:
					//TODO : implement
					break;
			}
		}
	}
}
