using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class ChangeActionMessage : QSBWorldObjectMessage<QSBGhostBrain, GhostAction.Name>
{
	public ChangeActionMessage(GhostAction.Name name) : base(name) { }

	public override void OnReceiveRemote()
	{
		DebugLog.DebugWrite($"{WorldObject.AttachedObject._name} Change action to {Data}");
		WorldObject.ChangeAction(WorldObject.GetAction(Data));
	}
}
