using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages
{
	internal class RetrieveProbeMessage : QSBBoolWorldObjectMessage<QSBProbeLauncher>
	{
		public RetrieveProbeMessage(bool playEffects) => Value = playEffects;

		public override void OnReceiveRemote() => WorldObject.RetrieveProbe(Value);
	}
}
