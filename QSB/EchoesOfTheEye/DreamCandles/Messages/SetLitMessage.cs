using QSB.EchoesOfTheEye.DreamCandles.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamCandles.Messages;

public class SetLitMessage : QSBWorldObjectMessage<QSBDreamCandle,
	(bool Lit, bool PlayAudio, bool Instant)>
{
	public SetLitMessage(bool lit, bool playAudio, bool instant)
		: base((lit, playAudio, instant))
	{ }

	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject.SetLit(Data.Lit, Data.PlayAudio, Data.Instant);
}
