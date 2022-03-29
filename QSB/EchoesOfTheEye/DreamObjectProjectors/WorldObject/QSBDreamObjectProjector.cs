using QSB.EchoesOfTheEye.DreamObjectProjectors.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;

public class QSBDreamObjectProjector : WorldObject<DreamObjectProjector>
{
	public override void SendInitialState(uint to)
		=> this.SendMessage(new ProjectorLitMessage(AttachedObject._lit) { To = to });
}
