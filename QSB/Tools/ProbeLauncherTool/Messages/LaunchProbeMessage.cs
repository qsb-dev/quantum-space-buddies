using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages;

public class LaunchProbeMessage : QSBWorldObjectMessage<QSBProbeLauncher, (bool playEffects, uint probeOwnerID)>
{
	public LaunchProbeMessage(bool playEffects, uint probeOwnerID) : base((playEffects, probeOwnerID)) { }

	public override void OnReceiveRemote() => WorldObject.LaunchProbe(Data.playEffects, Data.probeOwnerID);
}