using QSB.EchoesOfTheEye.DreamRafts.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamRafts.WorldObjects;

public class QSBDreamRaftProjection : WorldObject<DreamRaftProjection>
{
	public override void SendInitialState(uint to) =>
		this.SendMessage(new UpdateVisibilityMessage(AttachedObject._visible, true));
}
