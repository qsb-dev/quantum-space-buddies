using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;

namespace QSB.Tools.ProbeLauncherTool.Messages;

public class ChangeModeMessage : QSBWorldObjectMessage<QSBProbeLauncher>
{
	public ChangeModeMessage() : base() { }

	public override void OnReceiveRemote() => WorldObject.ChangeMode();
}
