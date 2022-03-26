using QSB.EchoesOfTheEye.DreamRafts.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamRafts.WorldObjects;

public class QSBDreamObjectProjector : WorldObject<DreamObjectProjector>
{
	public override void SendInitialState(uint to) =>
		this.SendMessage(new SetLitMessage(AttachedObject._lit));
}
