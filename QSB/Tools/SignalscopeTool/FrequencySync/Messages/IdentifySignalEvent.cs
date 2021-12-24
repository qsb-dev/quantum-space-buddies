using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.SignalscopeTool.FrequencySync.Messages
{
	public class IdentifySignalEvent : QSBEvent<EnumMessage<SignalName>>
	{
		// TODO : fix this with save-sync
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<SignalName>.AddListener(EventNames.QSBIdentifySignal, Handler);
		public override void CloseListener() => GlobalMessenger<SignalName>.RemoveListener(EventNames.QSBIdentifySignal, Handler);

		private void Handler(SignalName name) => SendEvent(CreateMessage(name));

		private EnumMessage<SignalName> CreateMessage(SignalName name) => new()
		{
			AboutId = QSBPlayerManager.LocalPlayerId,
			EnumValue = name
		};

		public override void OnReceiveRemote(bool server, EnumMessage<SignalName> message)
		{
			PlayerData.LearnSignal(message.EnumValue);
			QSBEventManager.FireEvent("IdentifySignal");
			var displayMsg = $"{UITextLibrary.GetString(UITextType.NotificationSignalIdentified)} <color=orange>{AudioSignal.SignalNameToString(message.EnumValue)}</color>";
			var data = new NotificationData(NotificationTarget.All, displayMsg, 10f, true);
			NotificationManager.SharedInstance.PostNotification(data, false);
		}
	}
}