using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

internal class StopFacingMessage : QSBWorldObjectMessage<QSBGhostController>
{
	public override void OnReceiveRemote()
	{
		WorldObject.StopFacing(true);
	}
}
