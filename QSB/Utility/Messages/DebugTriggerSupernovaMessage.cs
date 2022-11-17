using QSB.Messaging;

namespace QSB.Utility.Messages;

public class DebugTriggerSupernovaMessage : QSBMessage
{
	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		PlayerData.SaveLoopCount(2);
		TimeLoop.SetTimeLoopEnabled(true);
		TimeLoop._isTimeFlowing = true;
		TimeLoop.SetSecondsRemaining(0);
	}
}
