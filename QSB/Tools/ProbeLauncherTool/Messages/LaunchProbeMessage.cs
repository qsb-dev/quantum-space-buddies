using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages;

internal class LaunchProbeMessage : QSBWorldObjectMessage<QSBProbeLauncher, (bool, uint)>
{
	public LaunchProbeMessage(bool playEffects, uint probeOwnerID) : base((playEffects, probeOwnerID)) { }

	public override void OnReceiveRemote() => WorldObject.LaunchProbe(Data.Item1, Data.Item2);
}