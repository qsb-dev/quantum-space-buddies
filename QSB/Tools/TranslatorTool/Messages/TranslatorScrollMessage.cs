using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.TranslatorTool.Messages;

public class TranslatorScrollMessage : QSBMessage<float>
{
	public TranslatorScrollMessage(float scrollPos) : base(scrollPos) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		var translator = (QSBNomaiTranslator)player.Translator;
		translator.SetScroll(Data);
	}
}
