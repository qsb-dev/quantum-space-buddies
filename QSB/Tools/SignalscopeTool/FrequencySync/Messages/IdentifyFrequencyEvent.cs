using QSB.Events;
using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.SignalscopeTool.FrequencySync.Messages
{
	public class IdentifyFrequencyEvent : QSBEvent<EnumMessage<SignalFrequency>>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<SignalFrequency>.AddListener(EventNames.QSBIdentifyFrequency, Handler);
		public override void CloseListener() => GlobalMessenger<SignalFrequency>.RemoveListener(EventNames.QSBIdentifyFrequency, Handler);

		private void Handler(SignalFrequency frequency) => SendEvent(CreateMessage(frequency));

		private EnumMessage<SignalFrequency> CreateMessage(SignalFrequency frequency) => new()
		{
			AboutId = QSBPlayerManager.LocalPlayerId,
			EnumValue = frequency
		};

		public override void OnReceiveRemote(bool server, EnumMessage<SignalFrequency> message)
		{
			PlayerData.LearnFrequency(message.EnumValue);
			var displayMsg = $"{UITextLibrary.GetString(UITextType.NotificationNewFreq)} <color=orange>{AudioSignal.FrequencyToString(message.EnumValue, false)}</color>";
			var data = new NotificationData(NotificationTarget.All, displayMsg, 10f, true);
			NotificationManager.SharedInstance.PostNotification(data, false);
		}
	}
}
