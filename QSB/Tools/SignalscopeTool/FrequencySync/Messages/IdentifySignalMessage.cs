using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Tools.SignalscopeTool.FrequencySync.Messages;

public class IdentifySignalMessage : QSBMessage<SignalName>
{
	public IdentifySignalMessage(SignalName name) => Data = name;

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		PlayerData.LearnSignal(Data);
		GlobalMessenger.FireEvent("IdentifySignal");
		var displayMsg = $"{UITextLibrary.GetString(UITextType.NotificationSignalIdentified)} <color=orange>{AudioSignal.SignalNameToString(Data)}</color>";
		var data = new NotificationData(NotificationTarget.All, displayMsg, 10f);
		NotificationManager.SharedInstance.PostNotification(data);
	}
}