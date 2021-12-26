using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages
{
	internal class RetrieveProbeMessage : QSBBoolWorldObjectMessage<QSBProbeLauncher>
	{
		public RetrieveProbeMessage(bool state) => Value = state;

		public RetrieveProbeMessage() { }

		public override void OnReceiveRemote() => WorldObject.RetrieveProbe(Value);
	}
}
