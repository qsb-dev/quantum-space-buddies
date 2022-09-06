using QSB.EchoesOfTheEye.DreamLantern.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamLantern.WorldObjects;

public class QSBDreamLantern : WorldObject<DreamLanternController>
{
	public override void SendInitialState(uint to)
	{
		this.SendMessage(new SetLitMessage(AttachedObject._lit) { To = to });
		this.SendMessage(new SetConcealedMessage(AttachedObject._concealed) { To = to });
		this.SendMessage(new SetFocusMessage(AttachedObject._focus) { To = to });
		this.SendMessage(new SetRangeMessage(AttachedObject._minRange, AttachedObject._maxRange) { To = to });
	}

	public bool IsGhostLantern => AttachedObject.name == "GhostLantern"; // it's as shrimple as that
}
