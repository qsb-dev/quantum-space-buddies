using QSB.Messaging;
using QSB.Patches;

namespace QSB.TimeSync.Messages;

/// <summary>
/// sent from non-host to host
/// </summary>
public class SetSecondsRemainingMessage : QSBMessage<float>
{
	public SetSecondsRemainingMessage(float secondsRemaining) : base(secondsRemaining) => To = 0;
	public override void OnReceiveRemote() => QSBPatch.RemoteCall(() => TimeLoop.SetSecondsRemaining(Data));
}
