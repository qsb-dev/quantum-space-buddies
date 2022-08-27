using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.Tools.ProbeLauncherTool.Messages;

internal class TakeSnapshotMessage : QSBWorldObjectMessage<QSBProbeLauncher>
{
	public TakeSnapshotMessage() : base() { }

	public override void OnReceiveRemote() => WorldObject.TakeSnapshot();
}
