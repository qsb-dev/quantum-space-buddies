using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Tools.SignalscopeTool.FrequencySync.Messages;

public class IdentifyFrequencyMessage : QSBMessage<SignalFrequency>
{
	public IdentifyFrequencyMessage(SignalFrequency frequency) => Value = frequency;

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		PlayerData.LearnFrequency(Value);
		var displayMsg = $"{UITextLibrary.GetString(UITextType.NotificationNewFreq)} <color=orange>{AudioSignal.FrequencyToString(Value, false)}</color>";
		var data = new NotificationData(NotificationTarget.All, displayMsg, 10f);
		NotificationManager.SharedInstance.PostNotification(data);
	}
}