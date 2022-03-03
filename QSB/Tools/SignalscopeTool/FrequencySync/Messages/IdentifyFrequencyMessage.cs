using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Tools.SignalscopeTool.FrequencySync.Messages
{
	public class IdentifyFrequencyMessage : QSBMessage<SignalFrequency>
	{
		public IdentifyFrequencyMessage(SignalFrequency frequency) => Data = frequency;

		public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			PlayerData.LearnFrequency(Data);
			var displayMsg = $"{UITextLibrary.GetString(UITextType.NotificationNewFreq)} <color=orange>{AudioSignal.FrequencyToString(Data, false)}</color>";
			var data = new NotificationData(NotificationTarget.All, displayMsg, 10f);
			NotificationManager.SharedInstance.PostNotification(data);
		}
	}
}