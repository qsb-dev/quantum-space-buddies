using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages;

internal class ChangeModeMessage : QSBWorldObjectMessage<QSBProbeLauncher>
{
	public ChangeModeMessage() : base() { }

	public override void OnReceiveRemote() => WorldObject.ChangeMode();
}
