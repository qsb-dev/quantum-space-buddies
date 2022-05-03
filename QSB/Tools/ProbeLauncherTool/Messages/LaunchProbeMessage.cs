using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages;

internal class LaunchProbeMessage : QSBWorldObjectMessage<QSBProbeLauncher, bool>
{
	public LaunchProbeMessage(bool playEffects) : base(playEffects) { }

	public override void OnReceiveRemote() => WorldObject.LaunchProbe(Data);
}