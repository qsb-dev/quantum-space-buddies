using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages
{
	internal class LaunchProbeMessage : QSBWorldObjectMessage<QSBProbeLauncher>
	{
		public override void OnReceiveRemote() => WorldObject.LaunchProbe();
	}
}
