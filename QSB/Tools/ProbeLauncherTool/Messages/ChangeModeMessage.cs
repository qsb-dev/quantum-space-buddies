using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.Tools.ProbeLauncherTool.Messages;

internal class ChangeModeMessage : QSBWorldObjectMessage<QSBProbeLauncher>
{
	public ChangeModeMessage() : base() { }

	public override void OnReceiveRemote() => WorldObject.ChangeMode();
}
