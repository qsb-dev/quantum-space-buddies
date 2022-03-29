using QSB.EchoesOfTheEye.DreamCandles.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamCandles.Messages;

public class SetLitMessage : QSBWorldObjectMessage<QSBDreamCandle,
	(bool lit, bool playAudio, bool instant)>
{
	public SetLitMessage(bool lit, bool playAudio = true, bool instant = false) :
		base((lit, playAudio, instant)) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetLit(Data.lit, Data.playAudio, Data.instant));
}
