using QSB.Messaging;

namespace QSB.Utility.Messages
{
	public class DebugTriggerSupernovaMessage : QSBMessage
	{
		public override void OnReceiveLocal() => OnReceiveRemote();
		public override void OnReceiveRemote() => TimeLoop.SetSecondsRemaining(0);
	}
}
