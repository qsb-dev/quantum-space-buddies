using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.FrequencySync.Events
{
	public class IdentifySignalEvent : QSBEvent<EnumMessage<SignalName>>
	{
		public override EventType Type => EventType.IdentifySignal;

		public override void SetupListener() => GlobalMessenger<SignalName>.AddListener(EventNames.QSBIdentifySignal, Handler);
		public override void CloseListener() => GlobalMessenger<SignalName>.RemoveListener(EventNames.QSBIdentifySignal, Handler);

		private void Handler(SignalName name) => SendEvent(CreateMessage(name));

		private EnumMessage<SignalName> CreateMessage(SignalName name) => new EnumMessage<SignalName>
		{
			AboutId = PlayerManager.LocalPlayerId,
			Value = name
		};

		public override void OnReceiveRemote(bool server, EnumMessage<SignalName> message)
		{
			PlayerData.LearnSignal(message.Value);
			EventManager.FireEvent("IdentifySignal");
			var displayMsg = $"{UITextLibrary.GetString(UITextType.NotificationSignalIdentified)} <color=orange>{AudioSignal.SignalNameToString(message.Value)}</color>";
			var data = new NotificationData(NotificationTarget.All, displayMsg, 10f, true);
			NotificationManager.SharedInstance.PostNotification(data, false);
		}
	}
}