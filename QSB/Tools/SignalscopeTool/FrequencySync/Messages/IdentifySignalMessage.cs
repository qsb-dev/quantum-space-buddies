using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.Tools.SignalscopeTool.FrequencySync.Messages
{
	public class IdentifySignalMessage : QSBEnumMessage<SignalName>
	{
		public IdentifySignalMessage(SignalName name) => Value = name;

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			PlayerData.LearnSignal(Value);
			GlobalMessenger.FireEvent("IdentifySignal");
			var displayMsg = $"{UITextLibrary.GetString(UITextType.NotificationSignalIdentified)} <color=orange>{AudioSignal.SignalNameToString(Value)}</color>";
			var data = new NotificationData(NotificationTarget.All, displayMsg, 10f);
			NotificationManager.SharedInstance.PostNotification(data);
		}
	}
}