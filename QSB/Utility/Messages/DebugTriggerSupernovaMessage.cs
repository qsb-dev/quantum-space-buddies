using QSB.Messaging;
using QSB.Patches;

namespace QSB.Utility.Messages;

public class DebugTriggerSupernovaMessage : QSBMessage
{
	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		PlayerData.SaveLoopCount(2);
		TimeLoop.SetTimeLoopEnabled(true);
		TimeLoop._isTimeFlowing = true;
		QSBPatch.RemoteCall(() => TimeLoop.SetSecondsRemaining(0));
	}
}
