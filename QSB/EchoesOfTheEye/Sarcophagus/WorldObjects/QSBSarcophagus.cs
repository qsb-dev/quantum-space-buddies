using QSB.EchoesOfTheEye.Sarcophagus.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Sarcophagus.WorldObjects;

public class QSBSarcophagus : WorldObject<SarcophagusController>
{
	public override void SendInitialState(uint to)
	{
		if (AttachedObject._isOpen || AttachedObject._isSlightlyOpen)
		{
			this.SendMessage(new OpenMessage());
		}
	}
}
