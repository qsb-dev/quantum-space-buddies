using QSB.EchoesOfTheEye.DreamCandles.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamCandles.WorldObjects;

public class QSBDreamCandle : WorldObject<DreamCandle>
{
	public override void SendInitialState(uint to) =>
		this.SendMessage(new SetLitMessage(AttachedObject._lit, false, true) { To = to });
}
